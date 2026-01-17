using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Mess_Management_System.Models;
using Mess_Management_System.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IO;
using Stripe;

namespace Mess_Management_System.Controllers
{
    [Authorize(Roles = "User")] // Only users can make payments
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeService _stripeService;

        public PaymentController(ApplicationDbContext context, StripeService stripeService)
        {
            _context = context;
            _stripeService = stripeService;
        }

        // Helper method to get current user info
        private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string GetCurrentUserEmail() => User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        private string GetCurrentUserName() => User.FindFirst(ClaimTypes.Name)?.Value ?? "User";

        /// <summary>
        /// Creates a Stripe Checkout Session and redirects user to payment page
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(int billId)
        {
            try
            {
                int userId = GetCurrentUserId();
                string userEmail = GetCurrentUserEmail();
                string userName = GetCurrentUserName();

                // Get the bill
                var bill = await _context.Bills
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                {
                    TempData["ErrorMessage"] = "Bill not found or you don't have permission to pay it.";
                    return RedirectToAction("Index", "UserBill");
                }

                if (bill.Paid)
                {
                    TempData["InfoMessage"] = "This bill has already been paid.";
                    return RedirectToAction("Details", "UserBill", new { id = billId });
                }

                // Create success and cancel URLs
                var domain = $"{Request.Scheme}://{Request.Host}";
                var successUrl = $"{domain}/Payment/Success?session_id={{CHECKOUT_SESSION_ID}}";
                var cancelUrl = $"{domain}/Payment/Cancel?billId={billId}";

                // Create Stripe Checkout Session
                var session = await _stripeService.CreateCheckoutSession(
                    billId,
                    bill.Amount,
                    userEmail,
                    userName,
                    successUrl,
                    cancelUrl
                );

                // Redirect to Stripe Checkout
                return Redirect(session.Url);
            }
            catch (StripeException ex)
            {
                TempData["ErrorMessage"] = $"Payment error: {ex.Message}";
                return RedirectToAction("Index", "UserBill");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "UserBill");
            }
        }

        /// <summary>
        /// Success page after payment
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                TempData["ErrorMessage"] = "Invalid payment session.";
                return RedirectToAction("Index", "UserBill");
            }

            try
            {
                // Retrieve the session to get bill info
                var session = await _stripeService.GetSession(session_id);

                if (session.PaymentStatus == "paid")
                {
                    // Get bill ID from metadata
                    var billId = int.Parse(session.Metadata["bill_id"]);
                    var bill = await _context.Bills.FindAsync(billId);

                    if (bill != null && !bill.Paid)
                    {
                        // Mark bill as paid
                        bill.Paid = true;
                        _context.Bills.Update(bill);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Payment successful! Your bill has been marked as paid.";
                    }
                    else
                    {
                        TempData["InfoMessage"] = "This bill was already marked as paid.";
                    }
                }

                ViewBag.SessionId = session_id;
                ViewBag.AmountPaid = (session.AmountTotal / 100.0m); // Convert from cents
                ViewBag.Currency = session.Currency.ToUpper();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing payment: {ex.Message}";
                return RedirectToAction("Index", "UserBill");
            }
        }

        /// <summary>
        /// Cancel page if user cancels payment
        /// </summary>
        [HttpGet]
        public IActionResult Cancel(int billId)
        {
            TempData["InfoMessage"] = "Payment was cancelled. You can try again anytime.";
            return RedirectToAction("Details", "UserBill", new { id = billId });
        }

        /// <summary>
        /// Webhook endpoint for Stripe events (for production)
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeSignature = Request.Headers["Stripe-Signature"];
                if (string.IsNullOrEmpty(stripeSignature))
                {
                    return BadRequest();
                }
                var stripeEvent = _stripeService.ConstructWebhookEvent(json, stripeSignature);

                // Handle the event - FIXED LINE
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (session?.PaymentStatus == "paid")
                    {
                        var billId = int.Parse(session.Metadata["bill_id"]);
                        var bill = await _context.Bills.FindAsync(billId);

                        if (bill != null && !bill.Paid)
                        {
                            bill.Paid = true;
                            _context.Bills.Update(bill);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }
    }
}