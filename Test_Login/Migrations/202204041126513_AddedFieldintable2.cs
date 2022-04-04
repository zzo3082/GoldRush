namespace Test_Login.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFieldintable2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "UserTier", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "UserTier");
        }
    }
}
