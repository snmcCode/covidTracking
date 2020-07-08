# COVID-19 TRACKING APP API DOCUMENTATION

// Generate table of contents here <br>

## Getting the ```Function Key```

In order to get the function key, you will need to navigate to portal.azure.com and sign in with your credentials.

Once you are on the Azure Portal, click on the ```attendance-tracking``` resource group.

![attendancetracking](img/attendancetracking.png)

Next, scroll down and click on the ```snmtrackingapi``` resource.

![snmctrackingapi](img/snmctrackingapi.png)

Next, click on the ```Functions``` page in the side pane.

![snmctackingapi_functions](img/snmctrackingapi_functions.png)

From here, select the API that you need the function key for, in this case, we will be getting the function key for the ```RegisterVisitor``` API

![snmctracking_api_functions_select](img/snmctrackingapi_functions_select.png)

Next, click on the ```Function Keys``` page in the side pane.

![api_page](img/api_page.png)

Next, click on the word ```Default```.

![function_keys_page](img/function_keys_page.png)

Finally, click the ```Copy``` button next to the ```Value``` field to copy the ```Function Key```.

![function_keys_page_model](img/function_keys_page_modal.png)

---

## APIs <br>

## :dromedary_camel: Organization  <br>

## :pineapple: Retrieve Organization  

### Description

> Online test to retrieve a single organization by Id

### Request Type

> GET

### Request URL

> /api/organization/1


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

None

```

#### Optional Parameters

```json

None

```
<br><br>

## :pineapple: Register Organization  

### Description

> Online test for registering organization

### Request Type

> POST

### Request URL

> /api/organization


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

{
  "Name": "KMA"
}

```

#### Optional Parameters

```json

{
  "language": "json"
}

```
<br><br>

## :pineapple: Update Organization  

### Description

> 

### Request Type

> PUT

### Request URL

> /api/organization


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

{
  "Id": "id", 
  "Name": "OMA2"
}

```

#### Optional Parameters

```json

{
  "language": "json"
}

```
<br><br>

## :pineapple: Delete Organization  

### Description

> Online test to delete a single organization by Id

### Request Type

> DELETE

### Request URL

> /api/organization/2


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

None

```

#### Optional Parameters

```json

None

```
<br><br>

## :dromedary_camel: Visitor  <br>

## :pineapple: Retrieve Visitor  

### Description

> Online test to retrieve a single visitor by Id

### Request Type

> GET

### Request URL

> /api/user/{{myGUID}}


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

None

```

#### Optional Parameters

```json

None

```
<br><br>

## :pineapple: Retrieve Visitors  

### Description

> Online Test for Retrieving Visitors

### Request Type

> GET

### Request URL

> /api/users?Email=Ali.Baba@SNMC.ca


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

None

```

#### Optional Parameters

```json

None

```
<br><br>

## :pineapple: Register Visitor  

### Description

> Online test for registering visitor

### Request Type

> POST

### Request URL

> /api/user


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

{
  "Email": "Ali.Baba@SNMC.ca", 
  "FirstName": "Ali", 
  "IsMale": 1, 
  "IsVerified": 0, 
  "LastName": "Baba", 
  "PhoneNumber": "#########"
}

```

#### Optional Parameters

```json

{
  "language": "json"
}

```
<br><br>

## :pineapple: Update Visitor  

### Description

> Online test for updating visitors

### Request Type

> PUT

### Request URL

> /api/user


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

{
  "Email": "Ali.Baba@SNMC.ca", 
  "FirstName": "Ali", 
  "Id": "id", 
  "IsMale": 1, 
  "LastName": "Baba", 
  "PhoneNumber": "#########"
}

```

#### Optional Parameters

```json

{
  "language": "json"
}

```
<br><br>

## :pineapple: Delete Visitor  

### Description

> Online test to delete a single organization by Id

### Request Type

> DELETE

### Request URL

> /api/user/{{myGUID}}


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

None

```

#### Optional Parameters

```json

None

```
<br><br>

## :dromedary_camel: Visit Logging  <br>

## :pineapple: Log Visit 

### Description

> Online test for logging visit

### Request Type

> POST

### Request URL

> /api/visits


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

{
  "Direction": "In", 
  "Door": "North-West", 
  "Organization": "SNMC", 
  "VisitorId": "visitor alphanumeric id"
}

```

#### Optional Parameters

```json

{
  "language": "json"
}

```
<br><br>

## :dromedary_camel: SMS  <br>

## :pineapple: Verify SMS Verification Code  

### Description

> Online test for verifying SMS Verification Code

### Request Type

> POST

### Request URL

> /api/user/verify


### Custom Request Headers

```json
{
  "key": "x-functions-key", 
  "type": "text", 
  "value": "value"
}
```

### Request Body

#### Mandatory Parameters

```json

{
  "Id": "id", 
  "PhoneNumber": "#########", 
  "VerificationCode": "129632"
}

```

#### Optional Parameters

```json

{
  "language": "json"
}

```
<br>