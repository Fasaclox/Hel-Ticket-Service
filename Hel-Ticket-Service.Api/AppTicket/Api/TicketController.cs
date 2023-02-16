
using Microsoft.AspNetCore.Mvc;
using Hel_Ticket_Service.Domain;

namespace Hel_Ticket_Service.Api;
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        readonly ITicketRepository _ticketRepository ; 
        readonly ITicketService _ticketService;
        public TicketController(ITicketRepository ticketRepository, ITicketService ticketService) {
            _ticketRepository = ticketRepository;
            _ticketService    = ticketService;
        }
        [HttpPost]
        public async Task<ActionResult<string>> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            
                return Ok (await _ticketRepository.CreateTicket(createTicketDto));
            
        }
        [HttpPut("{reference}")]
        public async Task<ActionResult<string>> UpdateTicket(string reference,[FromBody] UpdateTicketDto updateTicketDto)
        {

                return Ok(await _ticketRepository.UpdateTicket(reference,updateTicketDto));

        }
        [HttpDelete("{reference}")]
        public async Task<ActionResult<string?>> DeleteTicket(string reference)
        {

                return Ok(await _ticketRepository.DeleteTicket(reference));
  
        }
        [HttpGet("reference/{reference}")]
        public async Task<ActionResult<Ticket>> GetTicketByReference(string reference)
        {
  
                return Ok( await _ticketRepository.GetTicketByReference(reference));
           
        }
        [HttpGet("category/{page:int:min(1)}")]
        public async Task<ActionResult<Ticket>> GetTicketByCategoryReference(string categoryreference, int page)
        {
  
                return Ok( await _ticketRepository.GetTicketByCategoryReference(categoryreference, page));
           
        }

        [HttpGet("search/{page:int:min(1)}")]
        public async Task<ActionResult<List<Ticket>>> SearchTicketList(string name, int page)
        {
                return Ok( await _ticketRepository.SearchTicketList(name, page));
          
        }
        [HttpGet("user/page/{page:int:min(1)}")]
        public async Task<ActionResult<List<Ticket>>> GetTicketByUserReference(string userReference, int page)
        {
                return Ok( await _ticketRepository.GetTicketByUserReference(userReference, page));
        }
        // [HttpGet("user/page/{page:int:min(1)}")]
        // public async Task<ActionResult<List<Ticket>>> GetALLTicketSByUser(string userReference, int page)
        // {
        //         return Ok( await _ticketRepository.GetTicketByUserReference(userReference, page));
        // }
        [HttpGet("user/category/page/{page:int:min(1)}")]
        public async Task<ActionResult<List<Ticket>>> GetTicketsByUserAndCategoryRef(string userReference, string categoryReference, int page)
        {
                return Ok( await _ticketRepository.GetTicketsByUserAndCategoryRef(userReference,categoryReference, page));
        }
        [HttpGet("user/status/page/{page:int:min(1)}")]
        public async Task<ActionResult<List<Ticket>>> GetTicketsByUserAndStatus(string userReference, string status, int page)
        {
                return Ok( await _ticketRepository.GetTicketsByUserAndStatus(userReference,status, page));
        }
        [HttpGet("user/category/status/page/{page:int:min(1)}")]
        public async Task<ActionResult<List<Ticket>>> GetTicketsByUserCategoryRefAndStatus(string userReference, string categoryRef, string status, int page)
        {
                return Ok( await _ticketRepository.GetTicketsByUserCategoryRefAndStatus(userReference, categoryRef, status, page));
        }
        // [HttpGet("user/status/page/{page:int:min(1)}")]
        // public async Task<ActionResult<List<Ticket>>> GetTicketsByUserReferenceAndStatus(string userReference, string status, int page)
        // {
        //         return Ok( await _ticketRepository.GetTicketsByUserReferenceAndStatus(userReference,status, page));
        // }
        [HttpGet("assigned/to/user/is-escalated{is_escalated}")]
        public async Task<ActionResult<List<Ticket>>> GetEscalatedTicketsByUser(string userReference)
        {
                return Ok( await _ticketRepository.GetEscalatedTicketsByUser(userReference));
        }
        [HttpGet("user/recent")]
        public async Task<ActionResult<List<Ticket>>> GetMostRecentTicketsByUser(string userReference)
        {
                return Ok( await _ticketRepository.GetMostRecentTicketsByUser(userReference));
        }
    }

