	public class CreatePostBusinessValidator : CommandBusinessValidator<CreatePost>
	{
		public override IEnumerable<ValidationResult> Validate(CreatePost create)
		{
			// Do funkiness
			return new ValidationResult[0];					
		}					
	}				
