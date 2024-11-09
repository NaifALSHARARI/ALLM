
# HakayaatBilArabiya Project

## Introduction
The HakayaatBilArabiya project is an interactive platform designed to generate Arabic educational stories and games using IBM Watson and Google Cloud technologies. The project uses natural language processing (NLP) and artificial intelligence to generate stories, perform grammatical analysis, and provide user interaction.

## Requirements
1. **.NET Core SDK** - Install the .NET Core SDK to run the application.
2. **Microsoft SQL Server** - Uses a local database (LocalDB).
3. **Python** - Required for running grammatical analysis using Camel Tools.
4. **IBM Watson and Google Cloud Credentials** - API keys for connecting to the required services.

## Setup
### Database Setup
- The project uses Microsoft SQL Server to store stories and comments.
- The database connection string is configured in the `appsettings.json` file under the `ConnectionStrings` section:

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HakayaatDb;Trusted_Connection=True;"
  },
  "IBMWatson": {
    "ApiKey": "z7xAbSwQrHVt8FwG8jVXY1W8zdGhKM0R3o4Zw0iKh5Y8",
    "ServiceUrl": "https://eu-de.ml.cloud.ibm.com/ml/v1/text/generation?version=2023-06-01",
    "ProjectId": "aa963093-70d1-4f8a-8dd9-5b43ec7278fd",
    "ModelId": "sdaia/allam-1-13b-instruct"
  },
  "GoogleCloud": {
    "CredentialFilePath": "wwwroot/credentials/google-credentials.json"
  }
}
IBM Watson Setup
•	The project uses IBM Watson for text generation and synonym analysis.
•	Configuration details are found in the appsettings.json file under the IBMWatson section:
•	{
•	  "Logging": {
•	    "LogLevel": {
•	      "Default": "Information",
•	      "Microsoft.AspNetCore": "Warning"
•	    }
•	  },
•	  "AllowedHosts": "*",
•	  "ConnectionStrings": {
•	    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HakayaatDb;Trusted_Connection=True;"
•	  },
•	  "IBMWatson": {
•	    "ApiKey": "z7xAbSwQrHVt8FwG8jVXY1W8zdGhKM0R3o4Zw0iKh5Y8",
•	    "ServiceUrl": "https://eu-de.ml.cloud.ibm.com/ml/v1/text/generation?version=2023-06-01",
•	    "ProjectId": "aa963093-70d1-4f8a-8dd9-5b43ec7278fd",
•	    "ModelId": "sdaia/allam-1-13b-instruct"
•	  },
•	  "GoogleCloud": {
•	    "CredentialFilePath": "wwwroot/credentials/google-credentials.json"
•	  }
•	}
•	ApiKey: The API key for IBM Watson.
•	ServiceUrl: The service URL.
•	ProjectId and ModelId: The project and model IDs for the specific service used.
Google Cloud Setup
•	The project uses Google Cloud for Text-to-Speech services.
•	The credential file path is specified in appsettings.json:
json
"GoogleCloud": {
    "CredentialFilePath": "wwwroot/credentials/google-credentials.json"
}
Project Structure
Files and Folders
•	Controllers: Contains main controllers like StoriesController.cs and GrammarStoriesController.cs for generating stories and analyzing texts.
•	Models: Holds data models like Story and Comment.
•	Services: Includes integrated services such as WatsonAssistantClient, AllamSynonymsService, and KnowledgeBaseService for communicating with IBM Watson and other services.
•	Views: Contains user interface files, including pages like Index.cshtml and GenerateStory.cshtml.
Key Files
StoriesController.cs
This file contains the StoriesController, which performs the following tasks:
•	Index: Displays all stored stories.
•	GenerateStory: Generates a new story based on user input using IBM Watson.
•	GetSynonyms: Retrieves synonyms for a specified word using the AllamSynonymsService.
•	Details: Displays details of a specific story.
•	Delete: Deletes a story from the database.
WatsonAssistantClient.cs
This file contains the WatsonAssistantClient service, which communicates with IBM Watson to perform the following tasks:
•	GetAccessTokenAsync: Retrieves an access token for IBM Watson.
•	GenerateTextAsync: Generates text using the AI model.
•	RemoveUnwantedPhrases: Cleans up unwanted phrases from generated texts.
arabic_gender_analyzer.py
Python script to analyze Arabic text and identify word types (masculine or feminine) using Camel Tools.
•	tokenize: Splits text into individual words.
•	analyze_text: Analyzes words and determines grammatical properties.
appsettings.json
Contains the main configuration settings for the project, including database connection and IBM Watson/Google Cloud credentials.
Running the Project
1.	Install Requirements: Ensure .NET Core, Python, and SQL Server are installed.
2.	Database Initialization: Set up the database by running the necessary Entity Framework commands (e.g., update-database).
3.	Run the Application:
o	Open the project in Visual Studio or any compatible development environment.
o	Ensure the appsettings.json file contains all required credentials.
o	Run the project (dotnet run or through Visual Studio).
Notes
•	Make sure to store API keys securely and avoid sharing them.
•	It is recommended to update credentials and appsettings.json configuration based on the environment (Development, Testing, or Production).
Common Issues
•	Database Connection Error: Check the connection string in appsettings.json.
•	IBM Watson or Google Cloud Access Error: Ensure that API keys and file paths are correctly configured.
Contributors
•	NAIF ALSHARARI - Lead Developer

