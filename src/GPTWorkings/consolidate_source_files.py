import os
import re
import glob

def should_ignore_line(line):
    # Regular expression to match lines that should be ignored
    ignore_patterns = [
        r'^\s*using',  # lines starting with 'using'
        #r'^\s*namespace',  # lines starting with 'namespace'
        r'^\s*/\*',  # lines starting with '/*'
        r'^\s*\*',  # lines starting with '*'
        r'^\s*\*/',  # lines starting with '*/'
        r'^\s*$'  # empty lines
    ]
    return any(re.match(pattern, line) for pattern in ignore_patterns)

def read_cs_files_and_combine(folder_path, output_file):
    # Ensure the folder path ends with a slash
    if not folder_path.endswith('\\'):
        folder_path += '\\'

    print('Read all files in ', folder_path)

    exclude_file_list = ['Module.cs', 'AssemblyAttributes.cs', 'Updater.cs']
    exclude_line_start_with = ("using", "namespace", "/*", "*/", "*")

    # Create or clear the output file
    with open(output_file, 'w', encoding='utf-8') as f:
        pass

    # Walk through the directory
    for root, dirs, files in os.walk(folder_path):
    
        # Skip 'bin' and 'obj' directories
        dirs[:] = [d for d in dirs if d not in ['bin', 'obj']]
        
        for file in files:
        
            # Check if the file is a .cs file
            if file.endswith('.cs') and file not in exclude_file_list:
                print(file)
                file_path = os.path.join(root, file)
                
                # Read the .cs file and append its content to the output file
                with open(file_path, 'r', encoding='utf-8-sig', errors='ignore') as file_reader:
                    lines = file_reader.readlines()
                
                    with open(output_file, 'a', encoding='utf-8') as file_writer:
                    
                        #file_writer.write(f"\n// Contents of {file_path}\n")
                        for line in lines:
                            # Use regex to check if the line should be ignored
                            if not should_ignore_line(line):
                                file_writer.write(line)

                        # Check if the last line ends with a newline, if not, add one
                        if lines and not lines[-1].endswith('\n'):
                            file_writer.write('\n')
                                
                        
                        #file_writer.write("\n\n")


# Usage
folder_path = 'C:\\dev\\client-development\\bosman-commerce7-sync\\src\\App\\BosmanCommerce7\\BosmanCommerce7.Module'  # Replace with your folder path
output_file = 'source.cs'
read_cs_files_and_combine(folder_path, output_file)
