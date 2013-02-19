using System.Web;
using System.Threading.Tasks;

namespace Web.Features.Documentation
{
	public class ResetContent : IHttpHandler
	{
		
		public virtual bool IsReusable {
			get {
				return true;
			}
		}
		
		
		public virtual void ProcessRequest (HttpContext context)
		{
			Parallel.Invoke (() => 
			{
				DocumentationContent.Reset ();
				DocumentationContent.Generate ();
			});

			context.Response.Charset = "UTF-8";
			context.Response.ContentType = "text/plain";
			context.Response.Write ("OK");
		}
	}
}

