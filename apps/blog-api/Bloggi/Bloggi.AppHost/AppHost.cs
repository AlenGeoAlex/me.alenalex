var builder = DistributedApplication.CreateBuilder(args);

const string API_REF = "me-alenalex-bloggi-api";
const string ANGULAR_REF = "me-alenalex-bloggi-angular";
const string NPGSQL_REF = "me-alenalex-bloggi-npgsql";
const string REDIS_REF = "me-alenalex-bloggi-redis";
const string NPGSQL_DB_NAME = "bloggi";

const string NPGSQL_USER = "bloggi_user";
const string NPGSQL_PASS = "bloggi_pass_123";

var postgresUserName = builder.AddParameter("username", NPGSQL_USER, secret: true);
var postgresPassword = builder.AddParameter("password", NPGSQL_PASS, secret: true);

var postgres = builder.AddPostgres(NPGSQL_REF, postgresUserName, postgresPassword)
    .WithDataVolume()
    .AddDatabase(NPGSQL_DB_NAME);

var api = builder.AddProject<Projects.Me_AlenAlex_Bloggi_Api>(API_REF)
    .WithReference(postgres)
    .WaitFor(postgres);

var frontEnd = builder.AddJavaScriptApp(ANGULAR_REF, "../../../angular-blog/")
    .WithReference(api)
    .WithPnpm(false)
    .WaitFor(api);
    

builder.Build()
    .Run();
