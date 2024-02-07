import json
import requests
import time
from requests.auth import HTTPBasicAuth

# Define the API endpoint
api_endpoint = "https://api.commerce7.com/v1/product"

# Basic Auth credentials
username = ""
password = ""

# Headers
headers = {
    "tenant": "neurasoft-sandbox",
    "Content-Type": "application/json"
}


# Function to remove unwanted keys and null values from the product data
#def remove_unwanted_properties(data):
#    if isinstance(data, dict):
#        # Remove keys in unwanted_keys list and any key with None as value
#        return {key: remove_unwanted_properties(value) for key, value in data.items() if key not in unwanted_keys and value is not None}
#    elif isinstance(data, list):
#        return [remove_unwanted_properties(item) for item in data]
#    else:
#        return data

def remove_unwanted_properties(data, unwanted_keys):
    if isinstance(data, dict):
        return {key: remove_unwanted_properties(value, unwanted_keys) for key, value in data.items() if key not in unwanted_keys and value is not None and not (isinstance(value, list) and len(value) == 0)}
    elif isinstance(data, list):
        return [remove_unwanted_properties(item, unwanted_keys) for item in data if item is not None and not (isinstance(item, list) and len(item) == 0)]
    else:
        return data

# Load the list of submitted product titles
submitted_titles_file = "submitted_titles.txt"
try:
    with open(submitted_titles_file, 'r', encoding='utf-8') as file:
        submitted_titles = file.read().splitlines()
except FileNotFoundError:
    submitted_titles = []

def store_sumitted_title(title):
    # Append the product title to the file
    with open(submitted_titles_file, 'a', encoding='utf-8') as file:
        file.write(title + "\n")

# Read product data from JSON file
filename = "C:\\dev\\client-development\\bosman-commerce7-sync\\Documents\\products\\Commerce7-products-production.json"

try:
    # List of unwanted key names to be removed
    unwanted_keys = ['id', 'createdAt', 'updatedAt', 'bundleItems', 'collections', 'wine', 'image']

    with open(filename, 'r', encoding='utf-8') as file:  # Specify the encoding here
        products = json.load(file)
    products = products['products']

    # Loop through each product in the JSON file
    for product in products:
    
        # Check if the product title is in the list of submitted titles
        if product.get("title") in submitted_titles:
            print(f"Product '{product.get('title')}' already submitted. Skipping.")
            continue

            
        # Remove any 'id' properties from the product data
        product_without_ids = remove_unwanted_properties(product, unwanted_keys)

        # Convert the product data to JSON
        product_payload = json.dumps(product_without_ids, indent=4)
        
        #print("product_payload")
        #print(product_payload)
        #print('-----')

        # Prompt for confirmation before sending the next product
        if 1 == 2:
            proceed = input("Proceed with sending the next product? ([y]/n/s): ")
            if proceed.lower() == 'n':
                print("Operation cancelled by user.")
                break
            elif proceed.lower() == 's':
                print("Skipped.")
                continue
            else:
                print()
                print("**********************************************************************************************************")
                print()

        # Sleep for a specified time before sending the next product
        time.sleep(0.500) 
        print("Send product: ", product_without_ids['title'])
        # Make a POST request to create the product
        response = requests.post(api_endpoint, headers=headers, data=product_payload, auth=HTTPBasicAuth(username, password))
        response_json = json.dumps(json.loads(response.text), indent=4)

        if response.status_code in [422]:
            if 'SKU is not unique' in response.text:
                store_sumitted_title(product.get("title"))
                print('SKU exists on Commerce7')
                continue
            if 'One or more elements is missing or invalid' in response.text:
                store_sumitted_title(product.get("title"))
                print('One or more elements is missing or invalid')
                print("**********************************************************************************************************")
                print("Response:")
                print(response_json)
                print("**********************************************************************************************************")
                
                continue

        # Check if the product was created successfully
        if response.status_code in [200, 201]:
            store_sumitted_title(product.get("title"))
            print("Product created successfully:")
            print()
            print(response_json)
            print()
            print("**********************************************************************************************************")
            print()
        else:
            print("**********************************************************************************************************")
            print("*** ERROR ***")
            print("**********************************************************************************************************")
            print("Failed to create product. Status code:", response.status_code)
            print()
            print("**********************************************************************************************************")
            print("product_payload")
            print(product_payload)
            print()
            print("**********************************************************************************************************")
            print("Response:")
            print(response_json)
            print("**********************************************************************************************************")
            print()
            break  # Terminate the script if the WEB API does not return HTTP 200

        # Sleep for a specified time before sending the next product
        time.sleep(2)  # Sleep for 2 seconds

except Exception as e:
    print("An error occurred:", str(e))
    exit()  # Terminate the script if there is an error
