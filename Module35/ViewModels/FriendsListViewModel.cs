using Module35.Models;

namespace Module35.ViewModels;

public class FriendsListViewModel
{
    public List<FriendViewModel> Friends { get; set; } = new List<FriendViewModel>();
    public List<FriendViewModel> PendingRequests { get; set; } = new List<FriendViewModel>();
    public List<FriendViewModel> IncomingRequests { get; set; } = new List<FriendViewModel>();
    public List<FriendViewModel> OutgoingRequests { get; set; } = new List<FriendViewModel>();
    public string SearchQuery { get; set; } = string.Empty;
    public List<FriendViewModel> SearchResults { get; set; } = new List<FriendViewModel>();
} 