	public class CreatePostInputValidator : CommandInputValidator<CreatePost>
	{
		public CreatePostInputValidator()
		{
			RuleFor(c=>c.Title)
				.NotEmpty()
				.WithMessage("Title is required");				
		}					
	}				
