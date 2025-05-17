using System;

public class BankDataViewModel
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime AccountCreationDate { get; set; }

    public string RoadName { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
}