using FileSystem.Core.Entities;
using System.ComponentModel;
using System.Linq.Expressions;

namespace FileSystem.Core.Models
{
	public class SortCriteria
	{
		public Expression<Func<Content, object>> Selector { get; private set; }
		public ListSortDirection SortDirection { get; private set; }

		public SortCriteria(Expression<Func<Content, object>> selector, ListSortDirection sortDirection)
		{
			Selector = selector;
			SortDirection = sortDirection;
		}
	}
}
