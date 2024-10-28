# PayPoint - Charge Notifiation Generator

Dani Drywa's solution to the PayPoint Charge Notification homework.


## Info

I have decided to use ASP.NET Core for this task, despite not having any prior experience with it. The web API is connecting to a mssqllocaldb instance that contains roughly 40k customers with a random amount of game charges across 3 days (25th, 26th, and 27th of October 2024). Generating documents for a single day (about 30k) takes about 42 seconds on my machine.

I used `QuestPDF` for the PDF generation. I had templating done via their fluent API because I do prefer using APIs over text files. However, I realised it might be a good idea to be able to change the template of the documents without recompiling the web server every time. So I have added a very crude XML template that translates to the FluentAPI. It is very limited and will most likely break in some edge cases but since the task didn't require it to be production ready I figured this should be good enough.

If you have any questions about this solution or just generally want to talk about it please feel free to get in touch :)


## Usage

A swagger instance is available at `https://localhost:7221/swagger/index.html`.

To generate charge notifications for ALL customers for a specific day POST the requested date to `/api/notification/{date}`. This will then start a backround task, generate the PDF documents, and store them in the configured output directory. By default this is the current process directory. The default output directory can be changed in the `appsettings.json` file:

```
"ChargeNotificationOptions": {
    "DocumentOutputDirectory": "C:\\Users\\micro\\Projects\\ChargeNotification\\WebAPI\\",
    "DocumentTemplateFilename": "C:\\Users\\micro\\Projects\\ChargeNotification\\WebAPI\\template.xml"
}
```

The `DocumentTemplateFilename` in the settings file points to a required `XML` template file that describes how the output document needs to look like.

`/api/notification/{date}` will return an `id` which can be used to track the status of the document generation via `/api/notification/{id}`. This will either return a `404 - Not Found` if the generation process is still in progress (or invalid), or a timestamp of the total runtime once the generation process has concluded. Sadly the server gets kind of hammered when processing the charge requests and the `/api/notification/{id}` seems to be taking a very long time to respond. My experience with ASP.NET Core is too little to know exactly that caused this and I couldn't figure it out during my time working on this challenge. I am sure I would be able to fix it eventually but for now, it is what it is.

`/api/notification/{id}/{date}` can be used to generate charge notification documents for a specific user `id`. However, this process is not tracked so you can not poll the generation status. It does however complete quite quickly anyways and will store the PDFs in the same configured `DocumentOutputDirectory`.

There are a few more endpoints available for testing purposes:

* `GET: /api/customer/{id}` To fetch a specific customer by `id`.
* `DELETE: /api/customer/{id}` To delete a specific customer by `id`.
* `POST: /api/customer/{name}` To create a new customer with a given `name`.
* `POST: /api/customer/create/{count}` To create a `count` of customers at once. (This was used to populate the database)

* `GET: /api/charge/{id}` To get a specific game charge by `id`.
* `DELETE: /api/charge/{id}` To delete a specific game charge by `id`.
* `GET: /api/charge/all/{id}/{date}` To get all game charges of a specific `date` for a specific customer `id`.
* `POST: /api/charge/create/{count}/{date}` To create a `count` of game charges for ALL customers at once. The `count` is randomised for every customer in the range of `[0..count)`. The game for each charge is also randomised. There are a total of `10` hardcoded games in this solution.
