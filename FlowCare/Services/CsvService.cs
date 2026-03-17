using System.Text;
using FlowCare.Models;

namespace FlowCare.Services
{
    public class CsvService
    {
        public byte[] ExportAuditLogs(List<AuditLog> logs)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Id,ActionType,UserId,UserRole,TargetEntity,TargetId,BranchId,CreatedAt,Metadata");

            foreach (var log in logs)
            {
            
                sb.AppendLine(
                    $"\"{log.Id}\"," +
                    $"\"{log.ActionType}\"," +
                    $"\"{log.UserId}\"," +
                    $"\"{log.UserRole}\"," +
                    $"\"{log.TargetEntity}\"," +
                    $"\"{log.TargetId}\"," +
                    $"\"{log.BranchId ?? ""}\"," +
                    $"\"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}\"," +
                    $"\"{log.Metadata?.Replace("\"", "'") ?? ""}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}