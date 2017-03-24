# SLOC-Counter
Counts number of source lines for Lykke Github

## Compilation
The program uses the library to access GitHub API located at https://github.com/octokit/octokit.net

## Running the program
At the start of program, one should enter the username/password for github access, these will not be stored anywhere, the reason is API rate limit

## Output
At the end of program execution, there will be a .txt and .csv output

## Method
The method used is to sum all additions and deletions in all commits. Similar to https://addons.mozilla.org/en-US/firefox/addon/github-sloc/
