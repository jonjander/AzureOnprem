
$adTenant = "tenantName.onmicrosoft.com"
$authority = "https://login.windows.net/$adTenant"
$clientId = "1950a258-ffff-ffff-ffff-717495945fc2"
$redirectUri = "urn:ietf:wg:oauth:2.0:oob"
$resourceAppIdURI = "https://management.azure.com/"

$token = Get-ADALToken -TenantId $adTenant -Authority $authority -Resource $resourceAppIdURI -ClientId $clientId -PromptBehavior Auto -RedirectUri $redirectUri 

"$($token.AccessTokenType) $($token.AccessToken)"| clip
