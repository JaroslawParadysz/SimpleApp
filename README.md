# SimpleApi

A simple .NET 8 minimal API that returns JSON data.

## API Endpoint

**GET** `/api/data` - Returns sample JSON data with message, timestamp, version, and items list.

## Running the API

```bash
cd src/SimpleApi
dotnet run
```

The API will be available at: `http://localhost:5267`

## Example Response

```json
{
  "message": "Hello from Simple API!",
  "timestamp": "2025-12-02T...",
  "version": "1.0.0",
  "items": [
    { "id": 1, "name": "Item One", "description": "First item" },
    { "id": 2, "name": "Item Two", "description": "Second item" },
    { "id": 3, "name": "Item Three", "description": "Third item" }
  ]
}
```

## Requirements

- .NET 8 SDK or later
