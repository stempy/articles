---
title: "Azure Active Directory Helpers"
categories:
  - Blog
tags:
  - link
  - Azure AD
---

On a recent project, I needed to write some Azure Active Directory functionality to help with retrieving user information via C#. Here is a helper I wrote.

First, we need a POCO (Plain Old C# Object)

```cs
// {"token_type":"Bearer","expires_in":"3599","ext_expires_in":"3599","expires_on":"1593997653","not_before":"1593993753","resource":"https://graph.microsoft.com",// "access_token":""
public class GraphToken
{
	[JsonPropertyName("token_type")]
	public string TokenType {get;set;}
	
	[JsonPropertyName("expires_in")]
	public string ExpiresIn {get;set;}

	[JsonPropertyName("expires_on")]
	public string ExpiresOn { get; set; }

	[JsonPropertyName("ext_expires_in")]
	public string ExtExpiresIn { get; set; }

	[JsonPropertyName("not_before")]
	public string NotBefore { get; set; }

	[JsonPropertyName("resource")]
	public string Resource {get;set;}

	[JsonPropertyName("access_token")]
	public string AccessToken {get;set;}

	public DateTime? ExpiresOnDate 
	{
		get 
		{
			if (string.IsNullOrEmpty(ExpiresOn))
				return null;

			if (double.TryParse(ExpiresOn, out double res)) 
			{
				return UnixTimeStampToDateTime(res);
			};
			return null;
		}
	}

	private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
	{
		// Unix timestamp is seconds past epoch
		System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
		dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
		return dtDateTime;
	}
}
```

Than a poco for a user

```cs
    public class ActiveDirectoryUser
    {
        public string UPN { get; set; }
        public string DisplayName { get; set; }
        public string Message { get; set; }
        public string MobilePhone {get;set;}
        public IEnumerable<string> BusinessPhones { get; set; }
        public ActiveDirectoryUser Manager { get; set; }
    }
```

Than the helper class itself

```cs
public class ActiveDirectoryHelpers
{
	private readonly string _tokenUrl;
	private readonly string _clientId;
	private readonly string _clientSecret;
	
	private GraphToken _token;
	
	private GraphServiceClient _graphService;
	
	public ActiveDirectoryHelpers(string tokenUrl, string clientId, string clientSecret) 
	{
		_tokenUrl = tokenUrl;
		_clientId = clientId;
		_clientSecret= clientSecret;
	}

	private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
	{
		// Unix timestamp is seconds past epoch
		System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
		dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
		return dtDateTime;
	}

	private async Task<GraphToken> GetActiveDirectoryTokenAsync()
	{
		if (_token == null)
		{
			var body = new Dictionary<string, string>
			{
				["grant_type"] = "client_credentials",
				["client_id"] = _clientId,
				["client_secret"] = _clientSecret,
				["resource"] = "https://graph.microsoft.com"
			};
			var client = new HttpClient();
			var c = new FormUrlEncodedContent(body);
			var response = await client.PostAsync(_tokenUrl, c);
			var json = await response.Content.ReadAsStringAsync();
			_token = JsonSerializer.Deserialize<GraphToken>(json);
			
			if (_token.ExpiresOnDate.HasValue){
				//_token.Dump("Token");
			}
			
		}
		return _token;
	}

	public async Task<GraphServiceClient> GetGraphServiceClient(string bearerToken = null)
	{
		if (_graphService==null)
		{
			var tokenDoc = await GetActiveDirectoryTokenAsync();
			bearerToken = bearerToken ?? tokenDoc.AccessToken;
			_graphService = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
					{
						requestMessage
							.Headers
							.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);

						return Task.FromResult(0);
					}));
		}
		return _graphService;
	}

	

	private async Task<IEnumerable<ActiveDirectoryUser>> Map(IGraphServiceUsersCollectionPage users)
	{
		var fnd = new List<ActiveDirectoryUser>();
		var graphService = await GetGraphServiceClient();
		
		async Task<ActiveDirectoryUser> GetUsersManager(string upn)
		{
			var directoryObject = await graphService.Users[upn].Manager.Request().GetAsync();
			if (directoryObject==null)
				return null;
			
			var mgr = directoryObject as User;

			var mgrAd = new ActiveDirectoryUser()
			{
				UPN = mgr.UserPrincipalName,
				BusinessPhones = mgr.BusinessPhones,
				MobilePhone = mgr.MobilePhone,
				DisplayName = mgr.DisplayName
			};
			return mgrAd;
		}

		if (users != null && users.Any())
		{
			var obj = users.Select(x => new ActiveDirectoryUser()
			{
				UPN = x.UserPrincipalName,
				DisplayName = x.DisplayName,
				MobilePhone = x.MobilePhone,
				BusinessPhones = x.BusinessPhones
			}).ToList();

			foreach (var o in obj)
			{
				var directoryObject = await graphService.Users[o.UPN].Manager.Request().GetAsync();
				var mgr = directoryObject as User;

				var mgrAd = new ActiveDirectoryUser()
				{
					UPN = mgr.UserPrincipalName,
					BusinessPhones = mgr.BusinessPhones,
					MobilePhone = mgr.MobilePhone,
					DisplayName = mgr.DisplayName
				};

				o.Manager = await GetUsersManager(o.UPN); //mgrAd;
			}

			fnd.AddRange(obj);
		}
		
		return fnd;
	}

	public async Task<IEnumerable<ActiveDirectoryUser>> FindUserDataByMobilePhones(IEnumerable<string> mobiles)
	{
		mobiles = mobiles.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
		
		var fnd = new List<ActiveDirectoryUser>();
		var graphService = await GetGraphServiceClient();
		foreach(var m in mobiles)
		{
			try
			{
				string filter = $"startsWith(mobilePhone,'{m}')";
				IGraphServiceUsersCollectionPage users = await graphService.Users.Request().Filter(filter).GetAsync();
				var obj = await Map(users);
				fnd.AddRange(obj);
			}
			catch (Exception ex)
			{
				fnd.Add(new ActiveDirectoryUser()
				{
					Message = ex.Message,
					MobilePhone = m
				});
			}
		}
		return fnd;
	}


	public async Task<IEnumerable<ActiveDirectoryUser>> FindUserDataByNamesOrUPN(IEnumerable<string> names)
	{
		names = names.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
		var fnd = new List<ActiveDirectoryUser>();
		var graphService = await GetGraphServiceClient();
		foreach (var n in names)
		{
			var isEmailAddress = n.Contains("@");
			
			try
			{
				
				string filter = isEmailAddress ? $"startsWith(userPrincipalName,'{n}')"
											   : $"startsWith(displayName, '{n}')";
											   
				IGraphServiceUsersCollectionPage users = await graphService.Users.Request().Filter(filter).GetAsync();
				var obj = await Map(users);
				if (!obj.Any())
				{
					throw new Exception($"NotFound");
				}
				
				
				fnd.AddRange(obj);
			}
			catch (Exception ex)
			{
				var ad = new ActiveDirectoryUser() 
				{
					Message = ex.Message
				};
				
				if (isEmailAddress){
					ad.UPN = n;
				} else {
					ad.DisplayName = n;
				}
				
				fnd.Add(ad);
			}
		}
		return fnd;
	}

	public async Task<DirectoryObject> GetUsersManagerByUPN(string upn)
	{
		var user = await GetUserByUPN(upn);
		var grpService = await GetGraphServiceClient();
		var directoryObject = await grpService.Users[upn].Manager
							.Request()
							.GetAsync();
		return directoryObject;
	}

	public async Task<User> GetUserByUPN(string UPN)
	{
		var graphServiceClient = await GetGraphServiceClient();
		var user = await graphServiceClient.Users[UPN].Request().GetAsync();
		return user;
	}

	public async Task<IEnumerable<User>> FindUsersByName(string name)
	{
		// Filter("startsWith(displayName, 'k')")
		var graphService = await GetGraphServiceClient();
		var users = await graphService.Users.Request().Filter($"startsWith(displayName, '{name}')").GetAsync();
		return users;
	}
}
```