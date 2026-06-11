namespace Audit.TimeCafe.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.EventType)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(x => x.Action)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(x => x.UserName)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(x => x.MachineName)
            .HasMaxLength(255);
        
        builder.Property(x => x.DomainName)
            .HasMaxLength(255);
        
        builder.Property(x => x.Exception)
            .HasColumnType("text");
        
        builder.Property(x => x.Duration);

        builder.Property(x => x.OldData)
            .HasColumnType("jsonb")
            .HasColumnName("old_data");
        
        builder.Property(x => x.NewData)
            .HasColumnType("jsonb")
            .HasColumnName("new_data");
        
        builder.Property(x => x.EnvironmentJson)
            .HasColumnType("jsonb")
            .HasColumnName("environment_json");
        
        builder.Property(x => x.CustomFieldsJson)
            .HasColumnType("jsonb")
            .HasColumnName("custom_fields_json");
        
        builder.Property(x => x.Comments)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
        
        builder.Property(x => x.StartDate);
        builder.Property(x => x.EndDate);
        
        builder.Property(x => x.CorrelationId)
            .HasMaxLength(36);

        builder.Property(x => x.UserId);
        
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.UserName);
        builder.HasIndex(x => x.CreatedAt).IsDescending();
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.UserId);
        
        builder.ToTable("audit_logs");
    }
}
