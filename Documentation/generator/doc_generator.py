from jinja2 import Environment, FileSystemLoader
import json
import re
from parser import *
import urllib
import urlparse

class DocGenerator:
    """ Generate a markdown document with info on APIs from postman exported file using jinja """ 

    _api_template = 'api_template.md'
    _group_template = 'group_template.md'
    _intro_template = 'intro_template.md'

    def __init__(self, data_file, dummy_data_file="dummy_vars.json"):
        self._dummy_data_file = dummy_data_file
        self._data_file = data_file

        with open(dummy_data_file) as f:
            self._dummy_vars_json = json.load(f)

        self._setup_jinja()

    def _setup_jinja(self, template_folder='templates'):
        self._template_folder_dest = template_folder

        # Load environment
        file_loader = FileSystemLoader(template_folder)
        env = Environment(loader=file_loader)

        # Set up filters
        env.filters['replace_vals_filter'] = self._replace_vals_filter
        env.filters['remove_test_keyword'] = self._remove_test_keyword
        env.filters['clean_escapes_filter'] = self._clean_escapes_filter
        env.filters['replace_url_params_filter'] = self._replace_url_params_filter

        # Get templates (intro isn't actually using jinja so loading it is unnecessary)
        self._apitemplate = env.get_template(self._api_template)
        self._group_template = env.get_template(self._group_template)

    def _replace_vals_filter(self, og_dict):
        ''' replace private info with those in dummy file '''
        for key in og_dict.keys():
            if key in self._dummy_vars_json:
                og_dict[key] = self._dummy_vars_json[key]
        return og_dict

    def _remove_test_keyword(self, string):
        ''' replace the test keyword in api name '''
        split_arr = string.split("_")
        new_string = string.replace("_", " ").replace("Testing", '').replace("Test", '').replace("Online", '')
        return new_string

    def _clean_escapes_filter(self, dirty_dict):
        ''' remove \n and \t '''
        clean_str = re.sub('\s+',' ',dirty_dict)
        return json.loads(clean_str)

    def _replace_url_params_filter(self, url):
        ''' replace params in url containing sensitive info with dummies '''
        parsed = urlparse.urlparse(url)
        query_dict = urlparse.parse_qs(parsed.query)
        for key in query_dict.keys():
            if key in self._dummy_vars_json:
                query_dict[key] = self._dummy_vars_json[key]

        new_url = list(parsed)
        new_url[4] = urllib.urlencode(query_dict)
        temp = urlparse.urlunparse(new_url)
        # return urlparse.unquote(temp) # this returns the entire url
        if new_url[4] != "":
            return new_url[2] + "?" + urlparse.unquote(new_url[4])
        return new_url[2]# return just the hierarchical path
      
    def generate_doc(self, destination_path):
        # Prep the output file
        Parser.clear_file_contents(destination_path)
        output_file = open(destination_path, "a")

        # Add the intro/tutorial stuff at the beginning
        intro_dest = self._template_folder_dest + "/" + self._intro_template
        with open(intro_dest ,"r") as f:
            output_file.write(f.read())

        # All folders and their relevant data in an array
        folders_arr = Parser.get_clean_json_from_file(self._data_file)

        # Pass each 'test' through the template and append to output file
        for folder in folders_arr:
            output_file.write(self._group_template.render(folder_name=folder["folder_name"]))
            for test in folder['tests']:
                output_file.write(self._apitemplate.render(test=test))
            
        # Close file
        output_file.close

DocGenerator('postmanFiles/SNMC.postman_collection.json').generate_doc('outputs/api_documentation.md')
