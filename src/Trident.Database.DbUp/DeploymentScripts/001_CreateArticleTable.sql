CREATE TABLE [dbo].[Article](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[Title] [nvarchar](1024) NOT NULL,
	[Content] [nvarchar](1024) NULL,
	[Url] [nvarchar](1024) NOT NULL,
	[FeedName] [nvarchar] (256) not null,
	[Thumbnail] [nvarchar](1024) NULL,
	[PublishedDate] [datetime] null
 CONSTRAINT [PK_Article] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) 