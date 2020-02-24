# Documentation

## Overview

This repo holds the markdown files of Steeltoe documentation in the `docs` sub-folder and a markdown parser console app. The app converts the markdown to HTML using the Markdig library and creates a json table of contents file.

The pipeline attached to this repo creates an artifact that holds the created files, in a specific folder structure. The `MainSite` repository contains the release pipelines that copy the contents of this artifact into the wwwroot folder during deployment.

## Directory Locations

* Documentation - All markdown documentation is located under the `docs` folder. These are the files that need to be modified for updates to the documentation section on the website.

* The markdown parser - This converts the markdown files to html and is located under `src`.

## Build

* Running the markdown parser
  * Usage: `> dotnet run --project src/ParseMD <location-of-markdown-files> <output-html-directory>`
  * Example Usage: `> dotnet run --project src/ParseMD <location-of-markdown-files> <output-html-directory>`

## Related Repository

* [MainSite](https://github.com/SteeltoeOSS/mainsite) - This repository contains the main site builder for steeltoe.io. It pulls in the html from the output of `ParseMD` and adds it to the documentation section of steeltoe.io.
