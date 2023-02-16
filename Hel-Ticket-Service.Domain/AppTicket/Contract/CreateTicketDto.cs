using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Hel_Ticket_Service.Domain.AppTicket.Entity.SubEntity;

namespace Hel_Ticket_Service.Domain;
public class CreateTicketDto
   { 
     public string Title { get; set; }
      public string CategoryReference { get; set; }
      public string Message {get;set;}
      public List<Image> Image {get;set;}
      public string UserReference {get;set;}
      public string Status {get; internal set;}
      public DateTime TimeStamp { get; set; }     
   }