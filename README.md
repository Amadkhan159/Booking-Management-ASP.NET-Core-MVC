# Booking-Management-ASP.NET-Core-MVC


📌 Functionality Overview: Booking System with Provider and Affiliate Panel
1️⃣ Search & Booking Module
🔹 Users access the platform to search for hotels, restaurants, or services.
🔹 Filtering options based on availability and location.
🔹 If a provider has no availability, it will not appear in the search results.
🔹 Users select a business and access its profile.
2️⃣ Provider Panel & Booking Options
🔹 Each provider has a control panel to manage their business details:
✅ Name, address, photo, description, and website link.
✅ Configuration of the “Book” button with multiple options:
•	📞 Direct call
•	💬 WhatsApp chat
•	✉ Email form submission
•	🖥 Interactive chat within the platform
•	🔗 Redirect to their own booking engine (e.g., Booking.com)
3️⃣ Booking Request Process
🔹 The user fills in their details and submits a request to the provider.
🔹 A record is automatically generated in:
📌 User panel (booking history).
📌 Provider panel (with request status: pending, confirmed, canceled).
🔹 Real-time notifications are sent to the provider and user via email/SMS.
4️⃣ Availability & Booking Management
🔹 Providers can configure daily booking limits based on their service type:
🏨 Hotels → Max number of rooms per day.
🍽 Restaurants → Number of tables available per time slot.
✂ Service-based businesses (salons, clinics, etc.) → Appointment scheduling by time slots.
🔹 Advanced scheduling options:
•	📆 Open/close specific days (e.g., closed on Sundays, open Saturdays only in the morning).
•	⏰ Define working hours per day (e.g., Monday-Friday 9:00 AM-6:00 PM, Saturday 9:00 AM-1:00 PM).
•	🚫 Block specific dates (holidays, vacations, special events).
🔹 When the booking availability reaches zero, the business will no longer appear in search results.
5️⃣ Request Counter & Billing System
🔹 Each time a user submits a booking request, the system tracks it.
🔹 A request counter is implemented for each provider.
🔹 Providers are billed monthly based on the number of requests received.
🔹 Optional: Purchase of request packages (e.g., 50, 100, 500 prepaid requests).
6️⃣ 📢 New: Affiliate Panel for Collaborators (Sales Partners)
🔹 Affiliate registration and login with a dedicated account.
🔹 Referral system:
•	Each affiliate receives a unique referral link.
•	If a provider signs up using that link, they are linked to the affiliate.
🔹 Subscription-based commission system:
•	Affiliates earn a percentage from each payment made by their referred providers.
•	Commissions can be one-time or recurring (e.g., 10% monthly while the provider remains subscribed).
🔹 Affiliate dashboard:
✅ List of referred providers.
✅ Payment and commission history.
✅ Withdrawal options (PayPal, bank transfer, etc.).
6️⃣ 📢 New:  Rating and Review System 
1. Customer Rating of the Provider 
•	Objective : Allow customers to rate their experience with the provider (hotel, restaurant, service) after completing a reservation or interaction. 
•	Key Features : 
o	Rating Scale : Customers can rate the provider on a scale of 1 to 5 stars.
	Evaluated aspects :
	Service quality.
	Cleanliness (especially relevant for hotels).
	Communication.
	Value for money.
o	Comment Field : Customers can leave a written review to provide details about their experience.
o	Visibility : Ratings and reviews will be visible on the provider’s profile for future customers.
o	Restrictions : Only customers who have completed a reservation or interaction will be able to leave a rating.

6️⃣ 📢 New:  Provider and Affiliate Account Approved by Admin
We need Separate Registration page for Provider and Affiliate every time when they are trying to Register the Request Send to Manager Pannel(admin) to Approved or Cancel the Request.

7️⃣ Security & Administration
🔹 Secure authentication with different user roles (users, providers, affiliates, admins).
🔹 Data protection and compliance with privacy regulations.
🔹 Balance and earnings management for providers and affiliates.


