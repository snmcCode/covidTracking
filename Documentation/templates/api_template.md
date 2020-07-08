<br>

## :pineapple: {{ test["test_name"] | remove_test_keyword }}

### Description

> {{ test["test_data"]["description"] }}

### Request Type

> {{ test["test_data"]["method"] }}

### Request URL

> {{ test["test_data"]["url"]["raw"] |replace_url_params_filter }}


### Custom Request Headers

```json
{{ test["test_data"]["header"][0] | replace_vals_filter | tojson(indent=2) }}
```

### Request Body

#### Mandatory Parameters

```json
{% if (test["test_data"]["method"] != 'GET') and (test["test_data"]["method"] != 'DELETE') %}
{{ test["test_data"]["body"]["raw"] | clean_escapes_filter | replace_vals_filter | tojson(indent=2) }}
{% else %}
{{ "None" }}
{% endif %}
```

#### Optional Parameters

```json
{% if (test["test_data"]["method"] != 'GET') and (test["test_data"]["method"] != 'DELETE') %}
{{ test["test_data"]["body"]["options"]["raw"] | replace_vals_filter | tojson(indent=2) }}
{% else %}
{{ "None" }}
{% endif %}
```
<br>