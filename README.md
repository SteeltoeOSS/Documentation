# Documentation

This repo holds the markdown files of Steeltoe documentation and a markdown parser console app. The app converts the markdown to HTML using the Markdig library and creates a json table of contents file.

The pipeline attached to this repo creates an artifact that holds the created files, in a specific folder structure. The MainSite release pipelines copy the contents of this artifact into the wwwroot folder during deployment.
