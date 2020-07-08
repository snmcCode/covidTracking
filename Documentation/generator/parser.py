import json

class Parser:
    """ Class with static methods to use for parsing """

    @staticmethod
    def get_clean_json_from_file(filename):
        with open(filename) as f:
            raw_api_data = json.load(f)

        ''' Filter the postman json file for only the info we want '''
        folders = raw_api_data["item"][0]["item"]
        clean_data = [] 

        for folder in folders:
            folder_dict = {}
            folder_name = folder["name"]
            folder_dict['folder_name'] = folder_name
            folder_dict['tests'] = []
            sub_folders = folder["item"]
            for sub_folder in sub_folders:
                if sub_folder["name"] == "Online":
                    for test in sub_folder["item"]:
                        test_dict = {}
                        test_dict['test_name'] = test["name"]
                        test_dict['test_data'] = test["request"]
                        folder_dict['tests'].append(test_dict)

            clean_data.append(folder_dict)
        
        return clean_data

    @staticmethod
    def debug(json_text):
        ''' Print the json file neatly '''
        print "\n\n FINAL:"
        for folder in json_text:
            print "\nFolder: " + folder["folder_name"]
            for test in folder['tests']:
                print "\nTest Name: " + test['test_name']
                print "\nTest Data: " 
                print test['test_data']

    @staticmethod
    def save_json_to_file(json_text, destination, to_append):
        ''' to_append is a bool for whether u want to append the json text to the end of the file or false if u want to overwrite.
         just for quick testing '''
        opt = "a" if to_append else "w"
        f = open(destination, "w")
        f.write(json.dumps(clean_data))
        f.close()

    @staticmethod
    def clear_file_contents(filepath):
        output_file = open(filepath, "w")
        output_file.write('')
        output_file.close()
