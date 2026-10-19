# Demo video script (about 90 seconds)

Record the screen at 1280 px width (browser window, not full screen). Use the English UI. Narrate in English or add subtitles. Keep the whole video under two minutes.

| Time | What to show | What to say / point out |
| --- | --- | --- |
| 0:00 | Home page | "Hearth & Bean is a demo bakery shop built with ASP.NET Core MVC and SQL Server. All data is fictional." |
| 0:10 | Menu: search "sourdough", filter a category, change sorting | "Search, category filter, sorting and pagination." |
| 0:25 | Open a product, add to bag as a guest | "Guests keep their bag in the session." |
| 0:35 | Register a new customer; open the bag | "After sign-in the guest bag is merged into the customer cart stored in SQL Server." |
| 0:50 | Checkout: choose Delivery, submit with an empty phone, then fix it and place the order | "Validation, then the order is created by the stored procedure sp_PlaceOrder in one transaction." |
| 1:05 | Order history, open the order, cancel it | "Customers see only their own orders; cancelling a Pending order restores the stock." |
| 1:15 | Sign in as admin: products, order statuses, reports | "Admins manage products and the order workflow and see three reports." |
| 1:30 | Switch to SSMS: run `SELECT * FROM dbo.vw_TopProducts` | "The reports are backed by views and procedures in the database." |

Upload the video to YouTube as "Unlisted" (or attach it to a GitHub Release) and put the link into `docs/demo-video-link.md`.
