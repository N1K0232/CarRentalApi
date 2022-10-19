namespace CarRentalApi.Shared.Common;

public class ListResult<TModel> where TModel : BaseModel
{
	public ListResult(IEnumerable<TModel> content)
	{
		Content = content;
		TotalCount = content?.Count() ?? 0;
		HasNextPage = false;
	}

	public ListResult(IEnumerable<TModel> content, int totalCount, bool hasNextPage = false)
	{
		Content = content;
		TotalCount = totalCount;
		HasNextPage = hasNextPage;
	}

	public IEnumerable<TModel> Content { get; }

	public int TotalCount { get; }

	public bool HasNextPage { get; }
}