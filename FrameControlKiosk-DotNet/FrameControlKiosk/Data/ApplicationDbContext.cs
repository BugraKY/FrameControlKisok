using FrameControlKiosk.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameControlKiosk.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<Control> Control { get; set; }
        public DbSet<Station> Station { get; set; }
        public DbSet<Definition> Definition { get; set; }
        public DbSet<ReportMain> ReportMain { get; set; }
        public DbSet<ReportDetail> ReportDetail { get; set; }
        public DbSet<Component> Component { get; set; }
        public DbSet<Coordinate> Coordinate { get; set; }
        public DbSet<CoordinateCheck> CoordinateCheck { get; set; }
        public DbSet<CoordinateDraws> CoordinateDraws { get; set; } //Migration a EKLENMEDİ!
    }
}
