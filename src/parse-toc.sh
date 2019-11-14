#!/bin/bash

set -e
set -o errtrace

export JSON="["

shopt -s dotglob
find * -prune -type d | while IFS= read -r d; do 
  export JSON=${JSON}'{
	"Link": "'${d}'",
	"Title": "XXXX",
	"MatchLink": "All",
	"MenuItems": ['
	cd ${d}
	for filename in *; do
		export JSON=${JSON}'{
			"Link": "'${d}'/'${filename}'",
			"Title": "XXXX",
			"MatchLink": "Prefix"
		},'
	done
	cd ../

	export JSON=${JSON}']},'
done
export JSON=${JSON}']'
echo $JSON
# json+="]"

# {
# 	"Link": "app-configuration",
# 	"Title": "Application Configuration",
# 	"MatchLink": "All",
# 	"MenuItems": [
# 		{
# 			"Link": "app-configuration/configuration-overview",
# 			"Title": "Overview",
# 			"MatchLink": "Prefix"
# 		},
# 		{
# 			"Link": "app-configuration/configuration-cloudfoundryprovider",
# 			"Title": "Cloud Foundry",
# 			"MatchLink": "Prefix",
# 			"MenuItems": [
# 				{
# 					"Link": "app-configuration/configuration-cloudfoundryprovider#add-nuget-reference",
# 					"Title": "Add NuGet Reference"
# 				},
# 				{
# 					"Link": "app-configuration/configuration-cloudfoundryprovider#add-configuration-provider",
# 					"Title": "Add Configuration Provider"
# 				}
# 			]
# 		},
# 		{
# 			"Link": "app-configuration/configuration-configserverprovider",
# 			"Title": "Spring Config Server",
# 			"MatchLink": "Prefix"
# 		},
# 		{
# 			"Link": "app-configuration/configuration-hostedextensions",
# 			"Title": "Hosted Extensions",
# 			"MatchLink": "Prefix"
# 		},
# 		{
# 			"Link": "app-configuration/configuration-placeholderyprovider",
# 			"Title": "Placeholders",
# 			"MatchLink": "Prefix"
# 		},
# 		{
# 			"Link": "app-configuration/configuration-randomvalueprovider",
# 			"Title": "Random Value",
# 			"MatchLink": "Prefix"
# 		}
# 	]
# }