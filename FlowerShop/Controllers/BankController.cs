using FlowerShop.Models.BankEntityFramework; 
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;


namespace BankingProject.Controllers

{
    public class BankController : Controller
    {
        //database is used to hold all of the bank database inforamtion
        private BANKDATAEntities1 database = new BANKDATAEntities1();

        //BankHomePage is main screen shown to new account holder to enter their account information
        public ActionResult BankHomePage()
        {
            return View();
        }
        //http post BankHomePage is used to handle the creation of an new bank user using the form that is in the homecontrollers
        //connected view
        [HttpPost]
        public ActionResult BankHomePage(FormCollection form)
        {
            //firstName holds the users table first name
            string firstName = form["first_name"];
            //lastName holds the users table last name
            string lastName = form["last_name"];
            //dateOfBirth holds the users table date of birth
            DateTime dateOfBirth = DateTime.Parse(form["date_of_birth"]);
            //roadName holds the addresses table road name aka address
            string roadName = form["road_name"];
            //city holds the addresses table city 
            string city = form["city"];
            //state is used to hold the state found within the address table
            string state = form["state"];
            //zipCode is used to hold the zip code found within the contactinfoes table
            string zipCode = form["zip_code"];
            //email is used to hold the email within the contactinfoes table
            string email = form["email_address"];
            //phone is used to hold the phone within the contactinfoes table
            string phone = form["phone_number"];
            //ssn is used to hold the ssn number within the contactinfoes table
            string ssn = form["social_security_number"];
            //makes sure we are using the right database
            using (var db = database)
            //begings the transaction about to be made within the account
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //bools are used to check if anyone else has the same email address, phone number or social security number
                    //since that would be impossible
                    bool emailExists = db.ContactInfoes.Any(c => c.email_address == email);
                    bool phoneExists = db.ContactInfoes.Any(c => c.phone_number == phone);
                    bool ssnExists = db.ContactInfoes.Any(c => c.social_security_number == ssn);
                    //the if statement checks each of the bools to see if there is a duplicate that does exist
                    if (emailExists || phoneExists || ssnExists)
                    {
                        //returns an error message about an duplicate
                        ModelState.AddModelError("", "A user with the same email, phone, or SSN already exists.");
                        return View("BankHomePage");
                    }
                    //newUserId is used to set the new user id within a newly created account
                    int newUserId = (db.Users.Any() ? Enumerable.Range(1, db.Users.Max(u => u.userid) + 1).Except(db.Users.Select(u => u.userid)).First() : 1);
                    //newUser is a refersense to a new User object
                    User newUser = new User
                    {
                        userid = newUserId,
                        first_name = firstName,
                        last_name = lastName,
                        date_of_birth = dateOfBirth,
                        account_creation_date = DateTime.Now
                    };
                    //adds the new user into the users table
                    db.Users.Add(newUser);
                    //saves the changes made
                    db.SaveChanges();
                    //userCreated checks to see if the new user that was created is created or not
                    bool userCreated = db.Users.Any(u => u.userid == newUserId);
                    //checks to see if the user is not been created within the users table
                    if (!userCreated)
                    {
                        //increase the newUserId
                        newUserId++;
                        //sets the new user id
                        newUser.userid = newUserId;
                        //attempts to put the new user id back into user
                        db.Users.Add(newUser);
                        //saves the changes made if it worked
                        db.SaveChanges();
                        //checks to see if the new user was not created
                        if (!db.Users.Any(u => u.userid == newUserId))
                        {
                            //sends an error message and rolls back an transactions done
                            ModelState.AddModelError("", "User creation failed after retry.");
                            transaction.Rollback();
                            return View("BankHomePage");
                        }
                    }
                    //newAddress is used to create a new Address object
                    var newAddress = new Address
                    {
                        address_id = newUserId,
                        userid = newUserId,
                        road_name = roadName,
                        city = city,
                        state = state,
                        zip_code = zipCode
                    };
                    //saves the changes made and done on addresses table
                    db.Addresses.Add(newAddress);
                    db.SaveChanges();
                    //newContact is used to create a new ContactInfo object
                    var newContact = new ContactInfo
                    {
                        contact_id = newUserId,
                        userid = newUserId,
                        email_address = email,
                        phone_number = phone,
                        social_security_number = ssn
                    };
                    //saves the changes made and done on to the contactinfoes table
                    db.ContactInfoes.Add(newContact);
                    db.SaveChanges();
                    //commits to all transactions done
                    transaction.Commit();
                    //users holds all the connected tables
                    var users = db.Users
                                  .Include("ContactInfoes")
                                  .Include("Addresses")
                                  .Include("Accounts")
                                  .ToList();
                    //is used to hold the newUserId within the application
                    ViewBag.SelectedUserId = newUserId;
                   

                    return View("BankHomePage", users);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "An error occurred while saving data: " + ex.Message);
                    return View("BankHomePage");
                }
            }
        }

        
        //BankDataView is used to edit an selected user and update or edit its information within the database
        public ActionResult BankDataView(string searchFilter, string searchText, User updatedUser)
        {
            //checks if the updatedUser is not equal to null and the user id is not equal to zero
            if (updatedUser != null && updatedUser.userid != 0)
            {
                //user is used to find and hold the updated user id
                var user = database.Users.Find(updatedUser.userid);
                //checks if the user is equal to null
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                }
                //if the user is not null
                else
                {
                    //hasChages is a boolean which keeps track of changes that may have been made within the view
                    bool hasChanges = false;
                    //each of the if statements are used to check if the results have changed
                    if (!string.Equals(user.first_name, updatedUser.first_name))
                    {
                        //sets the changed results and the chages stats
                        user.first_name = updatedUser.first_name;
                        hasChanges = true;
                    }
                    if (!string.Equals(user.last_name, updatedUser.last_name))
                    {
                        //sets the changed results and the chages stats
                        user.last_name = updatedUser.last_name;
                        hasChanges = true;
                    }
                    if (user.date_of_birth != updatedUser.date_of_birth)
                    {
                        //sets the changed results and the chages stats
                        user.date_of_birth = updatedUser.date_of_birth;
                        hasChanges = true;
                    }
                    if (user.account_creation_date != updatedUser.account_creation_date)
                    {
                        //sets the changed results and the chages stats
                        user.account_creation_date = updatedUser.account_creation_date;
                        hasChanges = true;
                    }
                    //contact is used to find and hold the contact within the user that might need to be updated
                    var contact = database.ContactInfoes.FirstOrDefault(c => c.userid == updatedUser.userid);
                    //updatedContact holds the newly added contact information
                    var updatedContact = updatedUser.ContactInfoes?.FirstOrDefault();
                    //checks if the contact and updatedContact are both not null
                    if (contact != null && updatedContact != null)
                    {
                        //each of the if statements checks for different errors that might occur
                        if (string.IsNullOrWhiteSpace(updatedContact.email_address) || !updatedContact.email_address.Contains("@"))
                        {
                            ModelState.AddModelError("", "A valid email is required.");
                            return View(updatedUser);
                        }
                        if (string.IsNullOrWhiteSpace(updatedContact.phone_number) || !System.Text.RegularExpressions.Regex.IsMatch(updatedContact.phone_number, "^\\d{3}-\\d{3}-\\d{4}$"))
                        {
                            ModelState.AddModelError("", "Phone number must be in the format ###-###-####.");
                            return View(updatedUser);
                        }
                        if (string.IsNullOrWhiteSpace(updatedContact.social_security_number) || !System.Text.RegularExpressions.Regex.IsMatch(updatedContact.social_security_number, "^\\d{3}-\\d{2}-\\d{4}$"))
                        {
                            ModelState.AddModelError("", "SSN must be in the format ###-##-####.");
                            return View(updatedUser);
                        }
                        //these if statements are used to finalize updates done within the contact
                        if (!string.Equals(contact.email_address, updatedContact.email_address))
                        {
                            contact.email_address = updatedContact.email_address;
                            hasChanges = true;
                        }
                        if (!string.Equals(contact.phone_number, updatedContact.phone_number))
                        {
                            contact.phone_number = updatedContact.phone_number;
                            hasChanges = true;
                        }
                        if (!string.Equals(contact.social_security_number, updatedContact.social_security_number))
                        {
                            contact.social_security_number = updatedContact.social_security_number;
                            hasChanges = true;
                        }
                    }
                    //address is used to hold the selected address from the user
                    var address = database.Addresses.FirstOrDefault(a => a.userid == updatedUser.userid);
                    //updatedAddress is used to hold the updated address information
                    var updatedAddress = updatedUser.Addresses?.FirstOrDefault();
                    //checks if the address and updatedAddress are not null
                    if (address != null && updatedAddress != null)
                    {
                        //if statements are used to finilize changes made if there where changes made
                        if (!string.Equals(address.road_name, updatedAddress.road_name))
                        {
                            address.road_name = updatedAddress.road_name;
                            hasChanges = true;
                        }
                        if (!string.Equals(address.city, updatedAddress.city))
                        {
                            address.city = updatedAddress.city;
                            hasChanges = true;
                        }
                        if (!string.Equals(address.state, updatedAddress.state))
                        {
                            address.state = updatedAddress.state;
                            hasChanges = true;
                        }
                        if (!string.Equals(address.zip_code, updatedAddress.zip_code))
                        {
                            address.zip_code = updatedAddress.zip_code;
                            hasChanges = true;
                        }
                    }
                    //checks if the hasChanges has been set to true
                    if (hasChanges)
                    {
                        try
                        {
                            //saves the changes made within the database
                            database.SaveChanges();
                            TempData["SuccessMessage"] = "User updated successfully.";
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "An error occurred while saving changes: " + ex.Message);
                        }
                    }
                }
            }
            //users is used to hold all the connected user information from the database to be filitered
            var users = database.Users
                .Include(u => u.ContactInfoes)
                .Include(u => u.Addresses)
                .AsQueryable();
            //if statement checks if the searchText is not null or empty
            if (!string.IsNullOrEmpty(searchText))
            {
                //coverts the text put within the searchText box to lower
                searchText = searchText.ToLower();
                //this put returns the searched information based off of what was put within the searchText string
                users = users.Where(u =>
                    string.IsNullOrEmpty(searchFilter) ||
                    (searchFilter == "UserID" && u.userid.ToString().Contains(searchText)) ||
                    (searchFilter == "FirstName" && u.first_name.ToLower().Contains(searchText)) ||
                    (searchFilter == "LastName" && u.last_name.ToLower().Contains(searchText)) ||
                    (searchFilter == "DateOfBirth" && u.date_of_birth.ToString("MM/dd/yyyy").Contains(searchText)) ||
                    (searchFilter == "AccountCreated" && u.account_creation_date.ToString("MM/dd/yyyy").Contains(searchText)) ||
                    (searchFilter == "Email" && u.ContactInfoes.Any(c => c.email_address.ToLower().Contains(searchText))) ||
                    (searchFilter == "Phone" && u.ContactInfoes.Any(c => c.phone_number.Contains(searchText))) ||
                    (searchFilter == "SSN" && u.ContactInfoes.Any(c => c.social_security_number.Contains(searchText))) ||
                    (searchFilter == "Address" && u.Addresses.Any(a => a.road_name.ToLower().Contains(searchText))) ||
                    (searchFilter == "City" && u.Addresses.Any(a => a.city.ToLower().Contains(searchText))) ||
                    (searchFilter == "State" && u.Addresses.Any(a => a.state.ToLower().Contains(searchText))) ||
                    (searchFilter == "Zipcode" && u.Addresses.Any(a => a.zip_code.Contains(searchText)))
                );
            }

            return View(users.ToList());
        }

        //UpdateUser is used to update the user table within the BankDataView
        [HttpPost]
        public IActionResult UpdateUser(User updatedUser)
        {
           //checks if the updated user is null or updatedUser is equal to 0
            if (updatedUser == null || updatedUser.userid == 0)
            {
                //display an error message about no user being selected
                ModelState.AddModelError("", "No user selected.");
                return ((IActionResult)View(updatedUser));
            }
            //user is used to hold the selected user to be updated
            var user = database.Users.Find(updatedUser.userid);
            //checks if the user is equal to null
            if (user == null)
            {
                //displays no user was found
                ModelState.AddModelError("", "User not found.");
                return (IActionResult)(View(updatedUser));
            }
            //hasChanges is used to check if changes have accorderd within the text inputs
            bool hasChanges = false;

            //the following if statements are used to check if changes where made
            if (!string.Equals(user.first_name, updatedUser.first_name))
            {
                user.first_name = updatedUser.first_name;
                hasChanges = true;
            }
            if (!string.Equals(user.last_name, updatedUser.last_name))
            {
                user.last_name = updatedUser.last_name;
                hasChanges = true;
            }
            if (user.date_of_birth != updatedUser.date_of_birth)
            {
                user.date_of_birth = updatedUser.date_of_birth;
                hasChanges = true;
            }
            if (user.account_creation_date != updatedUser.account_creation_date)
            {
                user.account_creation_date = updatedUser.account_creation_date;
                hasChanges = true;
            }

            //contact and updatedContact are both used to update the contacts side of the user within the database
            var contact = database.ContactInfoes.FirstOrDefault(c => c.userid == updatedUser.userid);
            var updatedContact = updatedUser.ContactInfoes?.FirstOrDefault();
            if (contact != null && updatedContact != null)
            {
                //the following if statements are used to check if changes where made
                if (!string.Equals(contact.email_address, updatedContact.email_address))
                {
                    contact.email_address = updatedContact.email_address;
                    hasChanges = true;
                }
                if (!string.Equals(contact.phone_number, updatedContact.phone_number))
                {
                    contact.phone_number = updatedContact.phone_number;
                    hasChanges = true;
                }
                if (!string.Equals(contact.social_security_number, updatedContact.social_security_number))
                {
                    contact.social_security_number = updatedContact.social_security_number;
                    hasChanges = true;
                }
            }

            //adddress and updatedAddress are used to update the useres address within the database
            var address = database.Addresses.FirstOrDefault(a => a.userid == updatedUser.userid);
            var updatedAddress = updatedUser.Addresses?.FirstOrDefault();
            if (address != null && updatedAddress != null)
            {
                //the following if statements are used to check if changes where made
                if (!string.Equals(address.road_name, updatedAddress.road_name))
                {
                    address.road_name = updatedAddress.road_name;
                    hasChanges = true;
                }
                if (!string.Equals(address.city, updatedAddress.city))
                {
                    address.city = updatedAddress.city;
                    hasChanges = true;
                }
                if (!string.Equals(address.state, updatedAddress.state))
                {
                    address.state = updatedAddress.state;
                    hasChanges = true;
                }
                if (!string.Equals(address.zip_code, updatedAddress.zip_code))
                {
                    address.zip_code = updatedAddress.zip_code;
                    hasChanges = true;
                }
            }
            //checks if hasChages is true
            if (hasChanges)
            {
                try
                {
                    //saves changes 
                    database.SaveChanges();
                    TempData["SuccessMessage"] = "User updated successfully.";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving changes: " + ex.Message);
                    return (IActionResult)View(updatedUser);
                }
            }

            return (IActionResult)RedirectToAction("BankDataView");
        }
        //DeleteUser within BankDataView is used to get the selected User by clicking on its id then safely deleting the selected users account information
        //along with any of the transactions occured within that account
        [HttpGet]
        public ActionResult DeleteUser(int userId)
        {
            //makes sure we are using the database for the bank 
            using (var db = database)
            {
                //user is used to get the user information as well as the account information
                var user = db.Users
                             .Include(u => u.Accounts.Select(a => a.Transactions))
                             .FirstOrDefault(u => u.userid == userId);
                //checks if the user is null
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("BankDataView");
                }
                //goes through each account that was created for the user
                foreach (var account in user.Accounts.ToList())
                {
                    //user is used to get the user information as well as the transaction information
                    var transactions = db.Transactions.Where(t => t.account_id == account.account_id).ToList();
                    //goes through each transaction 
                    foreach (var txn in transactions)
                    {
                        //removes the tranaction from the database
                        db.Transactions.Remove(txn);
                    }
                    //removes the account from the database
                    db.Accounts.Remove(account);
                }
                //contact is used to hold the contact information stored within the selected userid
                var contact = db.ContactInfoes.FirstOrDefault(c => c.userid == userId);
                //checks if contact is not null
                if (contact != null)
                    //removes the selected contact from the contants table
                    db.ContactInfoes.Remove(contact);
                //address is used to hold the address information stored within the selected userid
                var address = db.Addresses.FirstOrDefault(a => a.userid == userId);
                //checks if address is not null
                if (address != null)
                    //removes the selected address from the addresses table
                    db.Addresses.Remove(address);
                //removes the user from the users table
                db.Users.Remove(user);
                //finallize the deletion
                db.SaveChanges();
            }

            return RedirectToAction("BankDataView");
        }
        //BankAccountCreatorView is used to create, edit or delete bank accounts a bank account holder has based off the toggle button
        //which can either be account edit(delete and edit) or account create(create)
        public ActionResult BankAccountCreatorView(string searchFilter, string searchText, int? selectedUserId, int? selectedAccountId)
        {
            //users is used to hold all teh user information to be filitered
            var users = database.Users
                .Include(u => u.ContactInfoes)
                .Include(u => u.Addresses)
                .Include(u => u.Accounts)
                .AsQueryable();
            //is used to check if searchText is not equal to null or empty
            if (!string.IsNullOrEmpty(searchText))
            {
                //makes teh searchText lower case
                searchText = searchText.ToLower();
                //gets the users by what is put in the searchText string
                users = users.Where(u =>
                    string.IsNullOrEmpty(searchFilter) ||
                    (searchFilter == "UserID" && u.userid.ToString().Contains(searchText)) ||
                    (searchFilter == "FirstName" && u.first_name.ToLower().Contains(searchText)) ||
                    (searchFilter == "LastName" && u.last_name.ToLower().Contains(searchText)) ||
                    (searchFilter == "DateOfBirth" && u.date_of_birth.ToString("MM/dd/yyyy").Contains(searchText)) ||
                    (searchFilter == "AccountCreated" && u.account_creation_date.ToString("MM/dd/yyyy").Contains(searchText)) ||
                    (searchFilter == "Email" && u.ContactInfoes.Any(c => c.email_address.ToLower().Contains(searchText))) ||
                    (searchFilter == "Phone" && u.ContactInfoes.Any(c => c.phone_number.Contains(searchText))) ||
                    (searchFilter == "SSN" && u.ContactInfoes.Any(c => c.social_security_number.Contains(searchText))) ||
                    (searchFilter == "Address" && u.Addresses.Any(a => a.road_name.ToLower().Contains(searchText))) ||
                    (searchFilter == "City" && u.Addresses.Any(a => a.city.ToLower().Contains(searchText))) ||
                    (searchFilter == "State" && u.Addresses.Any(a => a.state.ToLower().Contains(searchText))) ||
                    (searchFilter == "Zipcode" && u.Addresses.Any(a => a.zip_code.Contains(searchText)))
                );
            }
            //creates an new account object
            Account selectedAccount = null;
            //checks to see if the selectedAccountId has an value in it
            if (selectedAccountId.HasValue)
            {
                //gets the selected account by its id
                selectedAccount = database.Accounts.FirstOrDefault(a => a.account_id == selectedAccountId);
            }
            //creates ViewBag vairable to be used to get different types of user and account ids within the 
            //webpage
            ViewBag.SelectedUserId = selectedUserId;
            ViewBag.SelectedAccountId = selectedAccountId;
            ViewBag.SelectedAccount = selectedAccount;

            return View(users.ToList());
        }
        //http post BankAccountCreatorView is used to get the information from the submited form and either create a new account or edit 
        //an new account
        [HttpPost]
        public ActionResult BankAccountCreatorView(int? account_id,int userid,string account_name,string account_desc,decimal balance,double interest_rate, string account_type)
        {
            //makes sure we are using the bank database
            using (var db = database)
            {
                //account creates a new Account object
                Account account = null;

                //is used to check if the account_id has a value and if its value is over 0
                if (account_id.HasValue && account_id.Value > 0)
                {
                    //account becomes the selected account using the account_id
                    account = db.Accounts.FirstOrDefault(a => a.account_id == account_id.Value);
                }
                //checks if the account is not equal to null
                if (account != null)
                {
                    //updates the account information to the selected account
                    account.userid = userid;
                    account.account_name = account_name;
                    account.account_desc = account_desc;
                    account.balance = balance;
                    account.interest_rate = (decimal?)interest_rate;
                    account.account_type = account_type;
                    account.account_activity = DateTime.Now;
                }
                //create an new account if the account is null
                else
                {
                    //nextAccountId becomes the account id for the newly generated account
                    int nextAccountId = (db.Accounts.Any() ? db.Accounts.Max(a => a.account_id) + 1 : 1);
                    //newAccount is used to create an new Account object
                    var newAccount = new Account
                    {
                        //sets in values for new account
                        account_id = nextAccountId,
                        userid = userid,
                        account_name = account_name,
                        account_desc = account_desc,
                        balance = balance,
                        interest_rate = (decimal?)interest_rate,
                        account_type = account_type,
                        account_activity = DateTime.Now
                    };
                    //adds the new account into the accounts database
                    db.Accounts.Add(newAccount);
                    account = newAccount;
                }
                //finalize database changes
                db.SaveChanges(); 
                //users is used to hold all the information to be displayed by the user side
                var users = db.Users
                              .Include(u => u.ContactInfoes)
                              .Include(u => u.Addresses)
                              .Include(u => u.Accounts)
                              .ToList();
                //viewbags are used to get account and user id information from the view page
                ViewBag.SelectedUserId = userid;
                ViewBag.SelectedAccountId = account?.account_id;
                ViewBag.SelectedAccount = account;

                return View(users);
            }
        }
        //DeleteAccount is used to delete an selected account by its id when the delete button is pressed 
        //which the mode of the page is in editing mode(this is done in the BankAccountCreatorView)
        [HttpGet]
        public ActionResult DeleteAccount(int accountId)
        {
            //makes sure we are using the bank database
            using (var db = database)
            {
                //account gets the account by the selected id
                var account = db.Accounts.FirstOrDefault(a => a.account_id == accountId);
                //checks if the account is equal to null
                if (account == null)
                {
                    TempData["Error"] = "Account not found.";
                    return RedirectToAction("BankAccountCreatorView");
                }
                //relatedTransactions is used to get all the transaction which where used on the selected account
                var relatedTransactions = db.Transactions.Where(t => t.account_id == accountId).ToList();
                //is used to loop through each transaction on the account and remove it
                foreach (var txn in relatedTransactions)
                {
                    db.Transactions.Remove(txn);
                }
                //removes the account from the Accounts table
                db.Accounts.Remove(account);
                //finalizes the changes made within the bank database
                db.SaveChanges();
            }

            return RedirectToAction("BankAccountCreatorView");
        }

        //TellerView is used to make transactions on user accounts like SW(Share Withdraw) SD(Share Deposit) TR(Share Transfer)
        public ActionResult TellerView()
        {
            //users is used to get all the user information needed to be displayed within the filitering table
            var users = database.Users
                .Include(u => u.ContactInfoes)
                .Include(u => u.Accounts.Select(a => a.Transactions))
                .ToList();

            return View(users);
        }
        //HttpGet handles the filitering logic that goes into finding certian accounts using a dynamic comma sepperated filitering system
        [HttpGet]
        public ActionResult TellerView(int? selectedAccountId, string searchFilter = null)
        {
            //users is used to get all the user information needed to be filitered within the filitering table
            var users = database.Users
                .Include(u => u.ContactInfoes)
                .Include(u => u.Accounts.Select(a => a.Transactions))
                .ToList();
            //checks if the searchFiliter is not null or empty
            if (!string.IsNullOrEmpty(searchFilter))
            {
                //filiters is used to get a list of different filitering information by putting a comma between each unqiue type of filiter
                var filters = searchFilter.ToLower().Split(',').Select(f => f.Trim()).ToList();
                //users is used to filiter out what was selected within the filiters
                users = users.Where(user =>
                {
                    //each var variable has its own list within its column which is used to filiter out muiltple types of
                    //information at once
                    var contact = user.ContactInfoes.FirstOrDefault();
                    var fullName = (user.first_name + " " + user.last_name).ToLower();
                    var phone = contact?.phone_number?.ToLower() ?? "";
                    var ssn = contact?.social_security_number ?? "";
                    var dob = user.date_of_birth.ToString("MM/dd/yyyy").ToLower();
                    //return is used to return the result of the muilti filitering
                    return filters.All(f =>
                        fullName.Contains(f) ||
                        phone.Contains(f) ||
                        dob.Contains(f) ||
                        ssn.StartsWith(f));

                }).ToList();
            }
            //checks if the selectedAccountId has a vlaue in it
            if (selectedAccountId.HasValue)
            {
                //account is used to hold all the account information from the database
                var account = database.Accounts
                    .Include(a => a.User)
                    .Include(a => a.User.Accounts.Select(t => t.Transactions))
                    .Include(a => a.Transactions)
                    .FirstOrDefault(a => a.account_id == selectedAccountId);
                //checks if account is not null
                if (account != null)
                {
                    //users creates a new list of users
                    users = new List<User> { account.User };
                    //viewbages are used to get the userid and sort the account transactions order by date
                    ViewBag.SelectedUserId = account.userid;
                    ViewBag.AccountTransactions = account.Transactions.OrderByDescending(t => t.transact_date).ToList();
                }
            }
            //is used to set the selected account id within the view
            ViewBag.SelectedAccountId = selectedAccountId;
            return View(users);
        }
        //http post in TellerView is used to submit transactions done on an account
        [HttpPost]
        public ActionResult TellerView(int accountId, string transactionTypes, FormCollection form)
        {
            //gets the selected account to do the transaction on
            var sourceAccount = database.Accounts.FirstOrDefault(a => a.account_id == accountId);
            //checks if the sourceAccount is equal to null
            if (sourceAccount == null)
            {
                //provides error message if the account id does not exist
                ModelState.AddModelError("accountId", "Account not found.");
                return RedirectToAction("TellerView");
            }
            //transactionList is used to put all transaction types within the list
            var transactionList = transactionTypes.ToUpper().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //isValid is used to check if a transaction is valid
            bool isValid = true;
            //goes through each transaction within the transactionList
            foreach (var transaction in transactionList)
            {
                //switch is used to handle what transaction was used 
                switch (transaction)
                {
                    case "SW":
                        //if statmenet is used to check if the amount put in can be converted to a decimal for an withdraw amount
                        if (decimal.TryParse(form["amount_SW"], out decimal withdrawAmount))
                        {
                            //checks to see if the sourceAccount balance is less than the withdrawAmount
                            if (sourceAccount.balance < withdrawAmount)
                            {
                                //generates error within the view not having enought funds for an withdrawal
                                ModelState.AddModelError("amount_SW", "Insufficient funds for withdrawal.");
                                //changes the transaction action to false
                                isValid = false;
                            }
                            //if the sourceAccount balance is greate than teh withdrawAmount
                            else
                            {
                                //subtracts the withdrawAmount from the sourceAccounts balance
                                sourceAccount.balance -= withdrawAmount;
                                //updates the sourceAccounts activity date and time
                                sourceAccount.account_activity = DateTime.Now;
                                //creates an new transaction entry within the bank database
                                database.Transactions.Add(new Transaction
                                {
                                    account_id = sourceAccount.account_id,
                                    transact_name = "Share Withdraw",
                                    transact_desc = "User withdrawal",
                                    transact_type = "withdraw",
                                    transact_amount = withdrawAmount,
                                    transact_date = DateTime.Now
                                });
                            }
                        }
                        break;

                    case "SD":
                        //checks to see if the number put in can be converted to an decimal for an share deposit
                        if (decimal.TryParse(form["amount_SD"], out decimal depositAmount))
                        {
                            //adds the amount of money put in into the sourceAccounts balance
                            sourceAccount.balance += depositAmount;
                            //updates the sourceAccount acctivity date
                            sourceAccount.account_activity = DateTime.Now;
                            //adds the new transaction into the database
                            database.Transactions.Add(new Transaction
                            {
                                account_id = sourceAccount.account_id,
                                transact_name = "Share Deposit",
                                transact_desc = "User deposit",
                                transact_type = "deposit",
                                transact_amount = depositAmount,
                                transact_date = DateTime.Now
                            });
                        }
                        break;

                    case "TF":
                        //checks to see if the number put in can be converted to decimal for an transfer transaction
                        if (decimal.TryParse(form["amount_TF"], out decimal transferAmount) &&
                            int.TryParse(form["recipientAccountId_TF"], out int recipientId))
                        {
                            //gets the account that the amount is going to be transfered to
                            var destAccount = database.Accounts.FirstOrDefault(a => a.account_id == recipientId);
                            //checks if the destAccount is null
                            if (destAccount == null)
                            {
                                ModelState.AddModelError("recipientAccountId_TF", "Recipient account not found.");
                                isValid = false;
                            }
                            //checks if the sourceAccount balance is less than the transferAmount that was put in 
                            else if (sourceAccount.balance < transferAmount)
                            {
                                ModelState.AddModelError("amount_TF", "Insufficient funds for transfer.");
                                isValid = false;
                            }
                            //does the reset of the transfer transaction
                            else
                            {
                                //takes the transferAmount out of the source account
                                sourceAccount.balance -= transferAmount;
                                //puts the transferAmount into the account we want to money to go to
                                destAccount.balance += transferAmount;
                                //updates both account activitys dates and times
                                sourceAccount.account_activity = DateTime.Now;
                                destAccount.account_activity = DateTime.Now;
                                //adds the transactions done into the sourceAccount transactions
                                database.Transactions.Add(new Transaction
                                {
                                    account_id = sourceAccount.account_id,
                                    transact_name = "Transfer Out",
                                    transact_desc = $"Transferred to Account {recipientId}",
                                    transact_type = "transfer",
                                    transact_amount = transferAmount,
                                    transact_date = DateTime.Now
                                });
                                //adds the transactions done into the destAccount transactions
                                database.Transactions.Add(new Transaction
                                {
                                    account_id = destAccount.account_id,
                                    transact_name = "Transfer In",
                                    transact_desc = $"Received from Account {sourceAccount.account_id}",
                                    transact_type = "transfer",
                                    transact_amount = transferAmount,
                                    transact_date = DateTime.Now
                                });
                            }
                        }
                        break;

                    default:
                        ModelState.AddModelError("transactionTypes", $"Invalid transaction type: {transaction}");
                        isValid = false;
                        break;
                }
            }
            //checks if model state is valid and if the transaction was valid
            if (isValid && ModelState.IsValid)
            {
                //finalize changes and additions made to the bank database
                database.SaveChanges();
            }

            return RedirectToAction("TellerView");
        }

    }
    }


