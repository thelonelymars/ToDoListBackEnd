using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

// Register services to the DI container
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Specify allowed origin
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}   
app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
var item =new ItemTemplate("task1", "pending", 1);
var itemList = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.UseCors("http://localhost:4200");
var connectionString = "mongodb://127.0.0.1:27017/?directConnection=true&serverSelectionTimeoutMS=2000&appName=mongosh+2.3.9\n"; 
var client = new MongoClient(connectionString);
var database = client.GetDatabase("admin"); // Replace with your database name
var collection = database.GetCollection<MyDocument>("ToDoList"); // Replace with your collection name

var filter = Builders<MyDocument>.Filter.Empty;
var cursor = await collection.FindAsync(filter);
var documents = await cursor.ToListAsync();

Console.WriteLine(documents.ToJson());
// Print documents
foreach (var doc in documents)
{
    Console.WriteLine(doc.ToJson());
}

app.MapDelete("/Delete/{id}", async (string id) =>
{
     Console.WriteLine($"ObjectId('{id}')");
     var objectId = new ObjectId(id);
     Console.WriteLine(objectId);
     var filter = Builders<MyDocument>.Filter.Eq("_id", objectId);
     var result =  collection.DeleteOneAsync(filter);
 
});
app.MapPost("/AddItem", async (ItemTemplate item) =>
{
 
        var document = new MyDocument
        {
            Id = ObjectId.GenerateNewId(),  
            name = item.toDoListName,
            Status = item.status
        };
        await collection.InsertOneAsync(document);
    return Results.Ok("success");
    
 

 
});
app.MapPatch("/EditTask/{id}/{TaskName}", async (string id, string TaskName) =>
{
    var filter = Builders<MyDocument>.Filter.Eq("_id", ObjectId.Parse(id));
    collection.UpdateOne(filter,Builders<MyDocument>.Update.Set("name", TaskName));
    return Results.Ok("success");
});
 app.MapPost("/todo", async (List<ItemTemplate> newList) =>
{
    foreach (var List in newList)
    {
        // Create a new document for each item
        var document = new MyDocument
        {
            Id = ObjectId.GenerateNewId(),
            name = List.toDoListName,
            Status = List.status
        };

        // Insert the document into the collection
         await collection.InsertOneAsync(document);
    }
   
    
    // Replace the current list with the new one
    return Results.Ok("Your to do list item is saved"); // Respond with the updated list
});
 app.MapGet("/GetAllTaskList", async () =>
 {
     var collection = database.GetCollection<MyDocument>("ToDoList"); // Replace with your collection name

     var filter = Builders<MyDocument>.Filter.Empty;
     var cursor = await collection.FindAsync(filter);
     var documents = await cursor.ToListAsync();
     return documents.ToJson();
 });




app.Run();

public record ItemTemplate(string toDoListName, string status, short id);


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class MyDocument
{
    [BsonId]
    public ObjectId Id { get; set; }
    [BsonElement("name")]
    public string name { get; set; }
    [BsonElement("status")]
    public string Status { get; set; }
  
}



