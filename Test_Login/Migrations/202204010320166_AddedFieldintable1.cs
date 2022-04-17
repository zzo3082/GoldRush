namespace Test_Login.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFieldintable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "StockBag", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "StockBag");
        }
    }
}
