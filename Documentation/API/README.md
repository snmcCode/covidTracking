# API Documentation Generator :sparkles:

> Uses jinja2 to generate API Documentation from a file exported from Postman.
Run the doc_generator.py file to generate the document. 

Next Steps:
- [ ] Make json file for responses and use response template
- [x] Change 'test' keyword
- [x] Truncate URL
- [ ] Add URL params
- [ ] Remove the unnecessary request headers, only need the x-functions-key
- [ ] Merge mandatory and optional sections under request body into one section just called request body, the description will clarify what is optional and what is mandatory
- [ ] Status Codes Lookup Section - Parse the file called CustomStatusCodes.cs under the Common/Resources/ directory of the CovidTracking project, do include the PLACEHOLDER status code, as I want to keep it in case it somehow slips through
- [ ] Status Codes section under each API call, to do this, grab the API name (you will already have parsed this, ex. from Register_Organization_Test_Online to RegisterOrganization <-- This is the API name) and search for it under the BackEnd/ directory of the project, find which status codes are used by looking for the line StatusCode = .... You will need to lookup the name that is being called (it would be something like StatusCode = CustomStatusCodes.BADREQUESTBODY) and you need to reference it with the CustomStatusCodes file to get the StatusCode (you will find for example int BADREQUESTBODY = 400), and you need to copy the numbers into the Status Codes section for each API