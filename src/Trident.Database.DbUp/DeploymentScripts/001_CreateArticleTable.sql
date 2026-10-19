CREATE TABLE [dbo].[Article](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[Title] [nvarchar](1024) NOT NULL,
	[Url] [nvarchar](1024) NOT NULL,
	[FeedName] [nvharchar](256) not null,
	[ImageUrl] [nvarchar](1024) NULL,	
 CONSTRAINT [PK_Article] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) 