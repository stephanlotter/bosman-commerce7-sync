import json
import requests
import time

# Read product data from JSON file
filename = "C:\\dev\\client-development\\bosman-commerce7-sync\\Documents\\products\\Commerce7-products-production.json"

try:
    with open(filename, 'r', encoding='utf-8') as file:  # Specify the encoding here
        products = json.load(file)
    products = products['products']

    # Loop through each product in the JSON file
    sku_count = 0;
    #ss = ''
    for product in products:
        for variant in product.get("variants"):
            sku = variant.get("sku")
            print(sku)
            sku_count+=1
            #ss += "'" + sku + "',"

    print(f"sku count: {sku_count}")
    #print(ss)

except Exception as e:
    print("An error occurred:", str(e))
    exit()  # Terminate the script if there is an error
