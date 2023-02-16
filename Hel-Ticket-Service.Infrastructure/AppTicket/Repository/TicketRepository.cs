using Hel_Ticket_Service.Domain;
using MongoDB.Driver;
using Serilog;
using System.Text.Json;
using MongoDB.Bson;
using System.Collections.Generic;

namespace Hel_Ticket_Service.Infrastructure;
public class TicketRepository: ITicketRepository
{ 
    readonly IConfiguration _configuration;
    readonly ICacheProvider _cacheProvider;
    readonly IDBProvider _dbProvider;
    readonly IMongoCollection<Ticket> _ticket; 
    readonly ITicketService _ticketService;
  
    public TicketRepository(IConfiguration configuration,ICacheProvider cacheProvider, IDBProvider dbProvider, ITicketService ticketService)
    {
        _cacheProvider = cacheProvider;
        _dbProvider = dbProvider;
        _configuration = configuration;
        _ticketService = ticketService;
       
        _ticket =_dbProvider.Connect().GetCollection<Ticket>(nameof(Ticket).ToLower());
        var indexKeysDefinition = Builders<Ticket>.IndexKeys.Ascending(ticket => ticket.Title);
        _ticket.Indexes.CreateOneAsync(new CreateIndexModel<Ticket>(indexKeysDefinition,new CreateIndexOptions() { Unique = true }));
        
    }
    public async Task<string> CreateTicket(CreateTicketDto createTicketDto)
    {
        try
        {
            Log.Information("Validating input data {0} ...", createTicketDto);
            var validationError = _ticketService.ValidateCreateTicketDto(createTicketDto);
            if (validationError!= null) 
            {  
                Log.Error("Error validating input data: " + JsonSerializer.Serialize(createTicketDto));
                throw new AppException(validationError.ErrorData);
            }
            Log.Information("Mapping Ticket Data");
            var ticket = new Ticket(createTicketDto); //map the dto to the ticket object
            Log.Information("Inserting Ticket Data");
            await _ticket.InsertOneAsync(ticket);
            Log.Information("Data Inserted");
         
            return ticket.Reference;
        }
        catch (Exception e)
        {
            Log.Error("Error Creating Ticket: {0}", e.Message );
            throw new AppException(new[]{e.Message}, "DATABASE",500);
        }
    }
    public async Task<string?> UpdateTicket(string reference, UpdateTicketDto updateTicketDto)
    {
        try
        {
            Log.Information("Validating input data {0} ...", updateTicketDto);

            var validationError = _ticketService.ValidateUpdateTicketDto(updateTicketDto);
            if (validationError!= null) 
            {  
                Log.Error("Error validating input data: " + JsonSerializer.Serialize(updateTicketDto));
                throw new AppException(validationError.ErrorData);
            }

            Log.Information("Getting data by reference {0} ...", reference);
            var ticket = GetTicketByReference(reference).Result;
            Log.Information("Mapping Data");
            ticket = new Ticket(updateTicketDto);
            Log.Information("Updating Data");
            var result = await _ticket.ReplaceOneAsync(ticket => ticket.Reference == reference, ticket);
            Log.Information("Data Updated");
            
            return result.ModifiedCount != 0 ? reference: null;
        }
        catch (Exception e)
        {
            Log.Error("Error Updating Ticket: {0}", e.Message );
            throw new AppException(new[]{e.Message}, "DATABASE",500);
        }
    }
    public async Task<string?> DeleteTicket(string reference)
    {
        try
        {
            Log.Information("Getting data by reference {0} ...", reference);
            var ticket = GetTicketByReference(reference).Result;
            Log.Information("Deleting data");
            var result = await _ticket.DeleteOneAsync(ticket => ticket.Reference == reference);
            Log.Information("Data Deleted");
         
            return result.DeletedCount != 0? reference: null;
        }
        catch (Exception e)
        {
            Log.Error("Error Deleting Ticket: {0}", e.Message );
            throw new AppException(new[]{e.Message}, "DATABASE",500);
        }
    }
    public async Task<Ticket> GetTicketByReference(string reference)
    {
        try
        {
            Log.Information("Getting data by reference {0}", reference);
            var data = await _cacheProvider.GetFromCache<Ticket>(reference); // Get data from cache
            if (data is not null) return data;
            data = await _ticket.Find(ticket => ticket.Reference == reference).FirstOrDefaultAsync();
            //await _cacheProvider.SetToCache(reference,data); // Set cache
            return data;
        }
        catch (Exception e)
        {
            Log.Error("Error Getting Ticket: {0}", e.Message );
            throw new AppException(new[]{e.Message}, "DATABASE",500);
        }
    }

       
    public async Task<List<Ticket>> GetTicketByCategoryReference(string categoryreference, int page)
    {
        try
        {
            Log.Information("Searching data by page {0}", page);
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}"); // Get data from cache
            if (data is not null) return data;

            var filterBuilder = Builders<Ticket>.Filter;
            var ticketnameFilter = filterBuilder.Eq(ticket => ticket.CategoryReference, categoryreference);


            var filter = ticketnameFilter;

            data = await _ticket.Find(filter).Skip((page-1) * _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            // await _cacheProvider.SetToCache($"{page}",data,
            // TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));
            return data;
        }
        catch (Exception e)
        {
            Log.Error("Error Searching Ticket: {0}", e.Message );
            throw new AppException(new[]{e.Message}, "DATABASE",500);
        }
    }

    public async Task<List<Ticket>> SearchTicketList(string name, int page)
    {
        try
        {
            Log.Information("Searching data by page {0}", page);
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}"); // Get data from cache
            if (data is not null) return data;

            var filterBuilder = Builders<Ticket>.Filter;
            var ticketnameFilter = filterBuilder.Regex(ticket => ticket.Title, new BsonRegularExpression($"/{name}/"));


            var filter = ticketnameFilter;

            data = await _ticket.Find(filter).Skip((page-1) * _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            // await _cacheProvider.SetToCache($"{page}",data,
            // TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));
            return data;
        }
        catch (Exception e)
        {
            Log.Error("Error Searching Ticket: {0}", e.Message );
            throw new AppException(new[]{e.Message}, "DATABASE",500);
        }
    }
    public async Task<List<Ticket>> GetTicketByUserReference(string userReference, int page)
    {
        try
        {
            Log.Information($"searching data by page {page}");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}");
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);

            var filter = userReferenceFilter;
            data = await _ticket.Find(filter).Skip((page-1)* _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }
    public async Task<List<Ticket>> GetAllTicketsByUser(string userReference, int page)
    {
        try
        {
            Log.Information($"searching data by page {page}");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}");
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);

            var filter = userReferenceFilter;
            data = await _ticket.Find(filter).Skip((page-1)* _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }
    public async Task<List<Ticket>> GetTicketsByUserAndCategoryRef(string userReference, string CategoryReference, int page)
    {
        try
        {
            Log.Information($"searching data by page {page}");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}");
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);
            var categoryReferenceFilter = filterBuilder.Eq(x=> x.CategoryReference, CategoryReference);
            var filter = userReferenceFilter & categoryReferenceFilter;
            data = await _ticket.Find(filter).Skip((page-1)* _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }
     public async Task<List<Ticket>> GetTicketsByUserAndStatus(string userReference, string status, int page)
    {
        try
        {
            Log.Information($"searching data by page {page}");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}");
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);
            var statusFilter = filterBuilder.Eq(x=> x.Status, status);
            var filter = userReferenceFilter & statusFilter;
            data = await _ticket.Find(filter).Skip((page-1)* _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }
     public async Task<List<Ticket>> GetTicketsByUserCategoryRefAndStatus(string userReference, string categoryRef, string status, int page)
    {
        try
        {
            Log.Information($"searching data by page {page}");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}");
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);
            var statusFilter = filterBuilder.Eq(x=> x.Status, status);
            var categoryFilter = filterBuilder.Eq(x=> x.CategoryReference, categoryRef);
            var filter = (userReferenceFilter & statusFilter & categoryFilter);
            data = await _ticket.Find(filter).Skip((page-1)* _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }
     public async Task<List<Ticket>> GetTicketsByUserReferenceAndStatus(string userReference, string status, int page)
    {
        try
        {
            Log.Information($"searching data by page {page}");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>($"{page}");
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);
            var statusFilter = filterBuilder.Eq(x=> x.Status, status);
            var filter = userReferenceFilter & statusFilter;
            data = await _ticket.Find(filter).Skip((page-1)* _dbProvider.GetPageLimit())
            .Limit(_dbProvider.GetPageLimit()).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }
    public async Task<List<Ticket>> GetEscalatedTicketsByUser(string userReference)
    {
        try
        {
            Log.Information($"searching data by page");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>(userReference);
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);
            var escalatedFilter = filterBuilder.Eq(x=> x.IsEscalted, true);
            var filter = userReferenceFilter & escalatedFilter;
            data = await _ticket.Find(filter).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }
      public async Task<List<Ticket>> GetMostRecentTicketsByUser(string userReference)
    {
        try
        {
            Log.Information($"searching data by page");
            var data = await _cacheProvider.GetFromCache<List<Ticket>>(userReference);
            if(data is not null){
                return data;
            }
            var filterBuilder = Builders<Ticket>.Filter;
            var userReferenceFilter = filterBuilder.Eq(x=> x.UserReference, userReference);
            data = await _ticket.Find(userReferenceFilter).SortByDescending(x=>x.TimeStamp).Limit(10).ToListAsync();
            return data;
        }
        catch(Exception ex)
        {
            Log.Error($"error searching ticket: {ex.Message}");
            throw new AppException(new[]{ex.Message}, "DATABASE", 500);
        }
    }

}