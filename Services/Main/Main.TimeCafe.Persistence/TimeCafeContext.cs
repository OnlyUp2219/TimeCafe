namespace TimeCafe.Persistence;

public partial class TimeCafeContext : DbContext
{
    public TimeCafeContext()
    {
    }

    public TimeCafeContext(DbContextOptions<TimeCafeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BillingType> BillingTypes { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientAdditionalInfo> ClientAdditionalInfos { get; set; }

    public virtual DbSet<ClientStatus> ClientStatuses { get; set; }

    public virtual DbSet<FinancialTransaction> FinancialTransactions { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<PhoneConfirmation> PhoneConfirmations { get; set; }

    public virtual DbSet<Tariff> Tariffs { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Main.TimeCafe;Username=postgres;Password=");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingType>(entity =>
        {
            entity.HasKey(e => e.BillingTypeId).HasName("PK__BillingT__5233EF23A53230B5");

            entity.HasIndex(e => e.BillingTypeName, "IX_BillingTypes_Name")
                .IsUnique()
                .HasFilter("\"BillingTypeName\" IS NOT NULL");

            entity.Property(e => e.BillingTypeName)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            // Сидирование типов тарификации
            entity.HasData(
                new BillingType { BillingTypeId = 1, BillingTypeName = "Почасовая" },
                new BillingType { BillingTypeId = 2, BillingTypeName = "Поминутная" }
            );
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK__Clients__E67E1A244E706CCA");

            entity.HasIndex(e => e.AccessCardNumber, "IX_Clients_AccessCardNumber")
                .IsUnique()
                .HasFilter("\"AccessCardNumber\" IS NOT NULL");

            entity.HasIndex(e => e.Email, "IX_Clients_Email")
                .IsUnique()
                .HasFilter("\"Email\" IS NOT NULL");

            entity.HasIndex(e => e.PhoneNumber, "IX_Clients_PhoneNumber")
                .IsUnique()
                .HasFilter("\"PhoneNumber\" IS NOT NULL");

            entity.Property(e => e.AccessCardNumber).HasMaxLength(20).HasColumnType("varchar(20)");
            entity.Property(e => e.BirthDate).HasColumnType("date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnType("varchar(100)");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            entity.Property(e => e.LastName).HasMaxLength(50).HasColumnType("varchar(50)");
            entity.Property(e => e.MiddleName).HasMaxLength(50).HasColumnType("varchar(50)");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).HasColumnType("varchar(20)");
            entity.Property(e => e.Photo).HasColumnType("bytea");
            entity.Property(e => e.RefusalReason).HasMaxLength(1000).HasColumnType("text");

            entity.HasIndex(e => e.GenderId);
            entity.HasIndex(e => e.StatusId);

            entity.HasOne(d => d.Gender).WithMany(p => p.Clients)
                .HasForeignKey(d => d.GenderId)
                .HasConstraintName("FK__Clients__GenderI__2354350C");

            entity.HasOne(d => d.Status).WithMany(p => p.Clients)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__Clients__StatusI__24485945");
        });

        modelBuilder.Entity<ClientAdditionalInfo>(entity =>
        {
            entity.HasKey(e => e.InfoId).HasName("PK__ClientAd__4DEC9D7A0A15EECD");

            entity.ToTable("ClientAdditionalInfo");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.InfoText)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("text");

            entity.HasIndex(e => e.ClientId);

            entity.HasOne(d => d.Client).WithMany(p => p.ClientAdditionalInfos)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__ClientAdd__Clien__2818EA29");
        });

        modelBuilder.Entity<ClientStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__ClientSt__C8EE2063B82E1A46");

            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            // Сидирование статусов клиента
            entity.HasData(
                new ClientStatus { StatusId = 1, StatusName = "Черновик" },
                new ClientStatus { StatusId = 2, StatusName = "Активный" },
                new ClientStatus { StatusId = 3, StatusName = "Отказ от услуг" }
            );
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Financia__55433A6B57033591");

            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.TransactionDate, "IX_FinancialTransactions_TransactionDate");
            entity.HasIndex(e => e.TransactionTypeId);
            entity.HasIndex(e => e.VisitId);

            entity.Property(e => e.Amount).HasColumnType("numeric(10, 2)");
            entity.Property(e => e.Comment).HasColumnType("text");
            entity.Property(e => e.TransactionDate).HasColumnType("timestamp");

            entity.HasOne(d => d.Client).WithMany(p => p.FinancialTransactions)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Financial__Clien__4E3E9311");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.FinancialTransactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .HasConstraintName("FK__Financial__Trans__4F32B74A");

            entity.HasOne(d => d.Visit).WithMany(p => p.FinancialTransactions)
                .HasForeignKey(d => d.VisitId)
                .HasConstraintName("FK__Financial__Visit__5026DB83");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.GenderId).HasName("PK__Genders__4E24E9F72615BBD8");

            entity.Property(e => e.GenderName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            entity.HasIndex(e => e.GenderName).IsUnique().HasDatabaseName("UQ__Genders__F7C1771527CC73EB");

            // Сидирование полов
            entity.HasData(
                new Gender { GenderId = 1, GenderName = "Мужской" },
                new Gender { GenderId = 2, GenderName = "Женский" },
                new Gender { GenderId = 3, GenderName = "Не указан" }
            );
        });

        modelBuilder.Entity<PhoneConfirmation>(entity =>
        {
            entity.HasKey(e => e.ConfirmationId).HasName("PK__PhoneConf__9B4F4D6A6E9A7B4E");

            entity.Property(e => e.ConfirmationCode).HasMaxLength(10).HasColumnType("varchar(10)");
            entity.Property(e => e.GeneratedTime).HasColumnType("timestamp");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).HasColumnType("varchar(20)");

            entity.HasIndex(e => e.ClientId);

            entity.HasOne(d => d.Client).WithMany(p => p.PhoneConfirmations)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PhoneConf__Clien__5303482E");
        });

        modelBuilder.Entity<Tariff>(entity =>
        {
            entity.HasKey(e => e.TariffId).HasName("PK__Tariffs__E3E2E6E5A4F4E6F7");

            entity.HasIndex(e => e.BillingTypeId);
            entity.HasIndex(e => e.TariffName, "IX_Tariffs_TariffName")
                .IsUnique()
                .HasFilter("\"TariffName\" IS NOT NULL");
            entity.HasIndex(e => e.ThemeId);

            entity.Property(e => e.DescriptionTitle).HasMaxLength(100).HasColumnType("varchar(100)");
            entity.Property(e => e.Description).HasMaxLength(1000).HasColumnType("text");
            entity.Property(e => e.Icon).HasColumnType("bytea");
            entity.Property(e => e.TariffName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Price).HasColumnType("numeric(10,2)");

            entity.HasOne(d => d.BillingType).WithMany(p => p.Tariffs)
                .HasForeignKey(d => d.BillingTypeId)
                .IsRequired()
                .HasConstraintName("FK_Tariffs_BillingTypes");

            entity.HasOne(d => d.Theme).WithMany(p => p.Tariffs)
                .HasForeignKey(d => d.ThemeId)
                .HasConstraintName("FK__Tariffs__ThemeId__2BE97B0D");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.ThemeId).HasName("PK__Themes__FBB3E4D9A31F20F0");

            entity.HasIndex(e => e.TechnicalName, "IX_Themes_TechnicalName")
                .IsUnique()
                .HasFilter("\"TechnicalName\" IS NOT NULL");
            entity.HasIndex(e => e.ThemeName, "IX_Themes_ThemeName")
                .IsUnique()
                .HasFilter("\"ThemeName\" IS NOT NULL");

            entity.Property(e => e.TechnicalName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            entity.Property(e => e.ThemeName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            // Сидирование цветовых тем (из AddThemeSeeding)
            entity.HasData(
                new Theme { ThemeId = 1, ThemeName = "Красный", TechnicalName = "Red" },
                new Theme { ThemeId = 2, ThemeName = "Оранжевый", TechnicalName = "Orange" },
                new Theme { ThemeId = 3, ThemeName = "Янтарный", TechnicalName = "Amber" },
                new Theme { ThemeId = 4, ThemeName = "Желтый", TechnicalName = "Yellow" },
                new Theme { ThemeId = 5, ThemeName = "Лаймовый", TechnicalName = "Lime" },
                new Theme { ThemeId = 6, ThemeName = "Зеленый", TechnicalName = "Green" },
                new Theme { ThemeId = 7, ThemeName = "Изумрудный", TechnicalName = "Emerald" },
                new Theme { ThemeId = 8, ThemeName = "Бирюзовый", TechnicalName = "Teal" },
                new Theme { ThemeId = 9, ThemeName = "Голубой", TechnicalName = "Cyan" },
                new Theme { ThemeId = 10, ThemeName = "Небесный", TechnicalName = "Sky" },
                new Theme { ThemeId = 11, ThemeName = "Синий", TechnicalName = "Blue" },
                new Theme { ThemeId = 12, ThemeName = "Индиго", TechnicalName = "Indigo" },
                new Theme { ThemeId = 13, ThemeName = "Фиолетовый", TechnicalName = "Violet" },
                new Theme { ThemeId = 14, ThemeName = "Пурпурный", TechnicalName = "Purple" },
                new Theme { ThemeId = 15, ThemeName = "Фуксия", TechnicalName = "Fuchsia" },
                new Theme { ThemeId = 16, ThemeName = "Розовый", TechnicalName = "Pink" },
                new Theme { ThemeId = 17, ThemeName = "Роза", TechnicalName = "Rose" },
                new Theme { ThemeId = 18, ThemeName = "Сланец", TechnicalName = "Slate" },
                new Theme { ThemeId = 19, ThemeName = "Серый", TechnicalName = "Gray" },
                new Theme { ThemeId = 20, ThemeName = "Цинк", TechnicalName = "Zinc" },
                new Theme { ThemeId = 21, ThemeName = "Нейтральный", TechnicalName = "Neutral" },
                new Theme { ThemeId = 22, ThemeName = "Камень", TechnicalName = "Stone" }
            );
        });

        modelBuilder.Entity<TransactionType>(entity =>
        {
            entity.HasKey(e => e.TransactionTypeId).HasName("PK__Transact__20266D0B29C99FD2");

            entity.HasIndex(e => e.TransactionTypeName, "IX_TransactionTypes_Name")
                .IsUnique()
                .HasFilter("\"TransactionTypeName\" IS NOT NULL");

            entity.Property(e => e.TransactionTypeName)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            // Сидирование типов транзакций
            entity.HasData(
                new TransactionType { TransactionTypeId = 1, TransactionTypeName = "Пополнение" },
                new TransactionType { TransactionTypeId = 2, TransactionTypeName = "Списание" }
            );
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PK__Visits__4D3AA1DEECB6D8F4");

            entity.HasIndex(e => e.BillingTypeId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.EntryTime, "IX_Visits_EntryTime");
            entity.HasIndex(e => e.ExitTime, "IX_Visits_ExitTime");
            entity.HasIndex(e => e.TariffId);

            entity.Property(e => e.EntryTime).HasColumnType("timestamp");
            entity.Property(e => e.ExitTime).HasColumnType("timestamp");
            entity.Property(e => e.VisitCost).HasColumnType("numeric(10, 2)");

            entity.HasOne(d => d.BillingType).WithMany(p => p.Visits)
                .HasForeignKey(d => d.BillingTypeId)
                .HasConstraintName("FK__Visits__BillingT__4979DDF4");

            entity.HasOne(d => d.Client).WithMany(p => p.Visits)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Visits__ClientId__47919582");

            entity.HasOne(d => d.Tariff).WithMany(p => p.Visits)
                .HasForeignKey(d => d.TariffId)
                .HasConstraintName("FK__Visits__TariffId__4885B9BB");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
