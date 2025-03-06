# InitializrWeb

InitializrWeb is a web frontend for an Initializr deployment.
It uses InitializrApi-provided project metadata to populate its interface for easy perusal and selection by an end user.
After selecting desired project parameters, an end user uses InitializrWeb to submit project generation requests to the InitializrApi.

## Overview

![Steeltoe Initializr](./images/default.png)

The interface is made up of four areas:

* project configuration area
* project action area (bottom)
* UI configuration (right)
* external links (left)

The remainder of this document focuses on the project configuration and action areas.

## Project configuration

The configuration area exposes five project properties to the end user:

* **Name**
* **Namespace**: C# namespace
* **Application**: application name
* **Description**
* **Steeltoe**: Steeltoe version
* **.NET Framework**: .NET target framework
* **.NET template**
* **Dependencies**

## Project actions

The actions area provides three project actions to the end user:

* **Generate**
* **Explore**
* **Share**

### Generate

Clicking **Generate** submits the current configuration to the InitializrApi to generate a project archive.
The resultant project archive is a Zip file with a name based on the project name.

![Steeltoe Initializr generate](images/generate.png)

### Explore

Clicking **Explore** submits the current configuration to the InitializrApi to generate a project archive.
The resultant project archive is expanded in the UI so that the end user can explore the project.

![Steeltoe Initializr explore](images/explore.png)

### Share

Clicking **Share** displays a URL that represents the current project configuration.
It can be shared with other developers or saved in a bookmark.
Note that the URL is specific to InitializrWeb and cannot be used directly with the InitializrApi.

![Steeltoe Initializr share](images/share.png)
