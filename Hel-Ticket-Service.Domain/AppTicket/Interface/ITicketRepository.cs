namespace Hel_Ticket_Service.Domain;

public interface ITicketRepository{

    #region Database
    Task<string> CreateTicket(CreateTicketDto ticket);
    Task<string> UpdateTicket(string reference, UpdateTicketDto ticket);
    Task<string> DeleteTicket(string reference);
    Task<Ticket> GetTicketByReference(string reference);
    Task<List<Ticket>> GetTicketByCategoryReference(string categoryference, int page);
    //Task<List<Ticket>> GetTicketList(string name,int page);
    Task<List<Ticket>> SearchTicketList(string name,  int page);
    Task<List<Ticket>> GetTicketByUserReference(string userReference, int page);
    Task<List<Ticket>> GetAllTicketsByUser(string userReference, int page);
    Task<List<Ticket>> GetTicketsByUserAndCategoryRef(string userReference, string CategoryReference, int page);
    Task<List<Ticket>> GetTicketsByUserAndStatus(string userReference, string status, int page);
    Task<List<Ticket>> GetTicketsByUserCategoryRefAndStatus(string userReference, string categoryRef, string status, int page);
    Task<List<Ticket>> GetTicketsByUserReferenceAndStatus(string userReference, string status, int page);
    Task<List<Ticket>> GetEscalatedTicketsByUser(string userReference);
    Task<List<Ticket>> GetMostRecentTicketsByUser(string userReference);
    #endregion
}

