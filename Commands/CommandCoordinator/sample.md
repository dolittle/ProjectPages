	public class PostCommandHandlers : ICommandHandler
	{
		IAggregatedRootRepository<Post> _repository;
					
		public PostCommandHandlers(IAggregatedRootRepository<Post> repository)
		{
			_repository = repository;
		}
					
		public void Handle(CreatePost create)
		{
			var post = _repository.Get(create.PostId);
			post.Create(create.Title, create.Body);
		}
	}				
