CREATE TABLE [dbo].[Feed](
	[Id] [int] IDENTITY(1,1) NOT NULL,		
	[Name] [nvarchar](256) not null,
	[Url] [nvarchar](1024) NOT NULL,
	[RegExMatchingItems] [nvarchar](2048) NOT NULL,
 CONSTRAINT [PK_Feed] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) 