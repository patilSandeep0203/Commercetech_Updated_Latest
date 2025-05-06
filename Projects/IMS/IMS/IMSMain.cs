using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.Services.Security;
using Microsoft.VisualBasic;
using System.Collections;
using System.Data.SqlClient;

namespace IMS
{
    public partial class IMSMain : Form
    {
        public IMSMain()
        {
            InitializeComponent();
        }

        public MerchantApplication InstantiateMerchantApplication(int NumberOfPrincipals,
           int NumberOfBanks)
        {
            MerchantApplication ma = new MerchantApplication();
            ArrayList al = new ArrayList();
            int i = 0;

            //Instantiate Objects
            ma.Business = new Business();
            ma.Business.BusinessSectorPercentage = new BusinessSectorPercentage();
            ma.Business.LocationAddress = new Address();
            ma.Business.MailAddress = new Address();
            ma.Business.TerminalSectorPercentage = new TerminalSectorPercentage();
            al.Add(new CardType());
            al.Add(new CardType());
            al.Add(new CardType());
            ma.CardTypes = (CardType[])al.ToArray(typeof(CardType));
            al.Clear();
            ma.Financial = new Financial();
            for (i = 0; i < NumberOfBanks; i++)
            {
                Bank b = new Bank();
                b.Address = new Address();
                al.Add(b);
            }
            ma.Financial.Banks = (Bank[])al.ToArray(typeof(Bank));
            al.Clear();
            ma.Financial.CreditCard = new CreditCard();
            ma.History = new History();
            for (i = 0; i < NumberOfPrincipals; i++)
            {
                Principal p = new Principal();
                p.ResidenceAddress = new Address();
                al.Add(p);
            }
            ma.Principals = (Principal[])al.ToArray(typeof(Principal));
            al.Clear();

            // Initialize the defaults
            ma.AgentReferralID = "";
            ma.Business.AverageMonthlyVolume = 0;
            ma.Business.AverageSaleAmount = 0;
            ma.Business.BusinessSectorPercentage.Internet = 0;
            ma.Business.BusinessSectorPercentage.MailPhoneOrder = 0;
            ma.Business.BusinessSectorPercentage.Other = 0;
            ma.Business.BusinessSectorPercentage.Restaurant = 0;
            ma.Business.BusinessSectorPercentage.Retail = 0;
            ma.Business.BusinessSectorPercentage.Service = 0;
            ma.Business.EstablishmentDate = DateTime.MinValue;
            ma.Business.DBA = "";
            ma.Business.FaxNumber = "";
            ma.Business.FederalTaxID = "";
            ma.Business.LegalName = "";
            ma.Business.LocationAddress.Address1 = "";
            ma.Business.LocationAddress.Address2 = "";
            ma.Business.LocationAddress.City = "";
            ma.Business.LocationAddress.PostalCode = "";
            //ma.Business.LocationAddress.State = "";
            ma.Business.PhoneNumber = "";
            ma.Business.ProductLine = "";
            ma.Business.TerminalSectorPercentage.KeyedWithImprint = 0;
            ma.Business.TerminalSectorPercentage.KeyedWithoutImprint = 0;
            ma.Business.TerminalSectorPercentage.Swiped = 0;
            ma.Business.Type = BusinessType.SoleProprietorship;
            ma.Business.URL = "";
            ma.CardTypes[0].CardIssuer = CardIssuer.AmericanExpress;
            ma.CardTypes[0].AcceptAction = CardTypeOptions.DontApply;
            ma.CardTypes[0].AccountNumber = "";
            ma.CardTypes[1].CardIssuer = CardIssuer.Diners;
            ma.CardTypes[1].AcceptAction = CardTypeOptions.DontApply;
            ma.CardTypes[1].AccountNumber = "";
            ma.CardTypes[2].CardIssuer = CardIssuer.Discover;
            ma.CardTypes[2].AcceptAction = CardTypeOptions.DontApply;
            ma.CardTypes[2].AccountNumber = "";
            ma.EmailAddress = "";
            ma.ExternalAgentUserID = "";
            for (i = 0; i < ma.Financial.Banks.Length; i++)
            {
                Bank b = ma.Financial.Banks[i];
                b.AccountNumber = "";
                b.Address.Address1 = "";
                b.Address.Address2 = "";
                b.Address.City = "";
                b.Address.PostalCode = "";
                //b.Address.State = "";
                b.Name = "";
                b.PhoneNumber = "";
                b.RoutingNumber = "";
            }
            ma.Financial.CreditCard.CardholderName = "";
            ma.Financial.CreditCard.AccountNumber = "";
            ma.Financial.CreditCard.ExpirationDate = DateTime.MinValue;
            ma.History.HadPriorCardAquirer = false;
            ma.History.PriorCardAquirerDepartureReason = "";
            ma.History.PriorCardAquirerName = "";
            ma.History.TerminatedMerchantFile = false;
            for (i = 0; i < ma.Principals.Length; i++)
            {
                Principal p = ma.Principals[i];
                p.BirthDate = DateTime.MinValue;
                p.ResidenceEstablishmentDate = DateTime.MinValue;
                p.DriverLicenseExpirationDate = DateTime.MinValue;
                p.DriverLicenseNumber = "";
                //p.DriverLicenseState = "";
                p.FirstName = "";
                p.LastName = "";
                p.MiddleInitial = "";
                p.OwnershipPercentage = 0;
                p.ResidenceAddress.Address1 = "";
                p.ResidenceAddress.Address2 = "";
                p.ResidenceAddress.City = "";
                p.ResidenceAddress.PostalCode = "";
                //p.ResidenceAddress.State = "";
                p.ResidenceOwnership = ResidenceOwnership.Own;
                p.ResidencePhoneNumber = "";
                p.SocialSecurityNumber = "";
                p.Suffix = Suffix.None;
            }
            return ma;
        }
                
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            DataSet ds = GetIMSRecord(lblContactID.Text);
            if (ds != null)
            {
                DataRow dr = ds.Tables[0].Rows[0];

                //****************************************************************************************

                Merchant merchantRates = new Merchant();                
                
                MerchantApplication ma = InstantiateMerchantApplication(2, 1);
                ma.AgentReferralID = "a913794b-3";
                ma.EmailAddress = "jscott@ecenow.com";//dr["Email"].ToString().Trim()
                
                int i = 0;

                //Terminal Sector Percentage
                //ma.Business.TerminalSectorPercentage = new TerminalSectorPercentage();
                ma.Business.TerminalSectorPercentage.KeyedWithImprint = Convert.ToInt32(dr["ProcessPctKeyedwImprint"]);
                ma.Business.TerminalSectorPercentage.KeyedWithoutImprint = Convert.ToInt32(dr["ProcessPctKeyedwoImprint"]);
                ma.Business.TerminalSectorPercentage.Swiped = Convert.ToInt32(dr["ProcessPctSwiped"]);

                #region Business Info

                //Instantiate Objects
                //Mail Address
                ma.Business.MailAddress.Address1 = dr["BillingAddress"].ToString().Trim();
                ma.Business.MailAddress.Address2 = "";
                ma.Business.MailAddress.City = dr["BillingCity"].ToString().Trim();
                switch (dr["BillingState"].ToString().Trim().ToUpper())
                {
                    case "":
                        ma.Business.MailAddress.State = State.None;
                        break;
                    case "AK":
                        ma.Business.MailAddress.State = State.AK;
                        break;
                    case "AL":
                        ma.Business.MailAddress.State = State.AL;
                        break;
                    case "AR":
                        ma.Business.MailAddress.State = State.AR;
                        break;
                    case "AZ":
                        ma.Business.MailAddress.State = State.AZ;
                        break;
                    case "CA":
                        ma.Business.MailAddress.State = State.CA;
                        break;
                    case "CO":
                        ma.Business.MailAddress.State = State.CO;
                        break;
                    case "CT":
                        ma.Business.MailAddress.State = State.CT;
                        break;
                    case "DC":
                        ma.Business.MailAddress.State = State.DC;
                        break;
                    case "DE":
                        ma.Business.MailAddress.State = State.DE;
                        break;
                    case "FL":
                        ma.Business.MailAddress.State = State.FL;
                        break;
                    case "GA":
                        ma.Business.MailAddress.State = State.GA;
                        break;
                    case "HI":
                        ma.Business.MailAddress.State = State.HI;
                        break;
                    case "IA":
                        ma.Business.MailAddress.State = State.IA;
                        break;
                    case "ID":
                        ma.Business.MailAddress.State = State.ID;
                        break;
                    case "IL":
                        ma.Business.MailAddress.State = State.IL;
                        break;
                    case "IN":
                        ma.Business.MailAddress.State = State.IN;
                        break;
                    case "KS":
                        ma.Business.MailAddress.State = State.KS;
                        break;
                    case "KY":
                        ma.Business.MailAddress.State = State.KY;
                        break;
                    case "LA":
                        ma.Business.MailAddress.State = State.LA;
                        break;
                    case "MA":
                        ma.Business.MailAddress.State = State.MA;
                        break;
                    case "MD":
                        ma.Business.MailAddress.State = State.MD;
                        break;
                    case "ME":
                        ma.Business.MailAddress.State = State.ME;
                        break;
                    case "MI":
                        ma.Business.MailAddress.State = State.MI;
                        break;
                    case "MN":
                        ma.Business.MailAddress.State = State.MN;
                        break;
                    case "MO":
                        ma.Business.MailAddress.State = State.MO;
                        break;
                    case "MS":
                        ma.Business.MailAddress.State = State.MS;
                        break;
                    case "MT":
                        ma.Business.MailAddress.State = State.MT;
                        break;
                    case "NC":
                        ma.Business.MailAddress.State = State.NC;
                        break;
                    case "ND":
                        ma.Business.MailAddress.State = State.ND;
                        break;
                    case "NE":
                        ma.Business.MailAddress.State = State.NE;
                        break;
                    case "NH":
                        ma.Business.MailAddress.State = State.NH;
                        break;
                    case "NJ":
                        ma.Business.MailAddress.State = State.NJ;
                        break;
                    case "NM":
                        ma.Business.MailAddress.State = State.NM;
                        break;
                    case "NV":
                        ma.Business.MailAddress.State = State.NV;
                        break;
                    case "NY":
                        ma.Business.MailAddress.State = State.NY;
                        break;
                    case "OH":
                        ma.Business.MailAddress.State = State.OH;
                        break;
                    case "OK":
                        ma.Business.MailAddress.State = State.OK;
                        break;
                    case "OR":
                        ma.Business.MailAddress.State = State.OR;
                        break;
                    case "PA":
                        ma.Business.MailAddress.State = State.PA;
                        break;
                    case "RI":
                        ma.Business.MailAddress.State = State.RI;
                        break;
                    case "SC":
                        ma.Business.MailAddress.State = State.SC;
                        break;
                    case "SD":
                        ma.Business.MailAddress.State = State.SD;
                        break;
                    case "TN":
                        ma.Business.MailAddress.State = State.TN;
                        break;
                    case "TX":
                        ma.Business.MailAddress.State = State.TX;
                        break;
                    case "UT":
                        ma.Business.MailAddress.State = State.UT;
                        break;
                    case "VA":
                        ma.Business.MailAddress.State = State.VA;
                        break;
                    case "VT":
                        ma.Business.MailAddress.State = State.VT;
                        break;
                    case "WA":
                        ma.Business.MailAddress.State = State.WA;
                        break;
                    case "WI":
                        ma.Business.MailAddress.State = State.WI;
                        break;
                    case "WV":
                        ma.Business.MailAddress.State = State.WV;
                        break;
                    case "WY":
                        ma.Business.MailAddress.State = State.WY;
                        break;
                }//end switch
                ma.Business.MailAddress.PostalCode = dr["BillingZipCode"].ToString().Trim();

                //Percentages
                ma.Business.BusinessSectorPercentage.Internet = Convert.ToInt32(dr["BusinessPctInternet"]);
                ma.Business.BusinessSectorPercentage.MailPhoneOrder = Convert.ToInt32(dr["BusinessPctMailOrder"]);
                ma.Business.BusinessSectorPercentage.Other = Convert.ToInt32(dr["BusinessPctOther"]);
                ma.Business.BusinessSectorPercentage.Restaurant = Convert.ToInt32(dr["BusinessPctRestaurant"]);
                ma.Business.BusinessSectorPercentage.Retail = Convert.ToInt32(dr["BusinessPctRetail"]);
                ma.Business.BusinessSectorPercentage.Service = Convert.ToInt32(dr["BusinessPctService"]);

                //Location Address
                //ma.Business.LocationAddress = new Address();
                ma.Business.LocationAddress.Address1 = dr["Address"].ToString().Trim();
                ma.Business.LocationAddress.Address2 = "";
                ma.Business.LocationAddress.City = dr["City"].ToString().Trim();                
                switch (dr["State"].ToString().Trim().ToUpper())
                {
                    case "":
                        ma.Business.LocationAddress.State = State.None;
                        break;
                    case "AK":
                        ma.Business.LocationAddress.State = State.AK;
                        break;
                    case "AL":
                        ma.Business.LocationAddress.State = State.AL;
                        break;
                    case "AR":
                        ma.Business.LocationAddress.State = State.AR;
                        break;
                    case "AZ":
                        ma.Business.LocationAddress.State = State.AZ;
                        break;
                    case "CA":
                        ma.Business.LocationAddress.State = State.CA;
                        break;
                    case "CO":
                        ma.Business.LocationAddress.State = State.CO;
                        break;
                    case "CT":
                        ma.Business.LocationAddress.State = State.CT;
                        break;
                    case "DC":
                        ma.Business.LocationAddress.State = State.DC;
                        break;
                    case "DE":
                        ma.Business.LocationAddress.State = State.DE;
                        break;
                    case "FL":
                        ma.Business.LocationAddress.State = State.FL;
                        break;
                    case "GA":
                        ma.Business.LocationAddress.State = State.GA;
                        break;
                    case "HI":
                        ma.Business.LocationAddress.State = State.HI;
                        break;
                    case "IA":
                        ma.Business.LocationAddress.State = State.IA;
                        break;
                    case "ID":
                        ma.Business.LocationAddress.State = State.ID;
                        break;
                    case "IL":
                        ma.Business.LocationAddress.State = State.IL;
                        break;
                    case "IN":
                        ma.Business.LocationAddress.State = State.IN;
                        break;
                    case "KS":
                        ma.Business.LocationAddress.State = State.KS;
                        break;
                    case "KY":
                        ma.Business.LocationAddress.State = State.KY;
                        break;
                    case "LA":
                        ma.Business.LocationAddress.State = State.LA;
                        break;
                    case "MA":
                        ma.Business.LocationAddress.State = State.MA;
                        break;
                    case "MD":
                        ma.Business.LocationAddress.State = State.MD;
                        break;
                    case "ME":
                        ma.Business.LocationAddress.State = State.ME;
                        break;
                    case "MI":
                        ma.Business.LocationAddress.State = State.MI;
                        break;
                    case "MN":
                        ma.Business.LocationAddress.State = State.MN;
                        break;
                    case "MO":
                        ma.Business.LocationAddress.State = State.MO;
                        break;
                    case "MS":
                        ma.Business.LocationAddress.State = State.MS;
                        break;
                    case "MT":
                        ma.Business.LocationAddress.State = State.MT;
                        break;
                    case "NC":
                        ma.Business.LocationAddress.State = State.NC;
                        break;
                    case "ND":
                        ma.Business.LocationAddress.State = State.ND;
                        break;
                    case "NE":
                        ma.Business.LocationAddress.State = State.NE;
                        break;
                    case "NH":
                        ma.Business.LocationAddress.State = State.NH;
                        break;
                    case "NJ":
                        ma.Business.LocationAddress.State = State.NJ;
                        break;
                    case "NM":
                        ma.Business.LocationAddress.State = State.NM;
                        break;
                    case "NV":
                        ma.Business.LocationAddress.State = State.NV;
                        break;
                    case "NY":
                        ma.Business.LocationAddress.State = State.NY;
                        break;
                    case "OH":
                        ma.Business.LocationAddress.State = State.OH;
                        break;
                    case "OK":
                        ma.Business.LocationAddress.State = State.OK;
                        break;
                    case "OR":
                        ma.Business.LocationAddress.State = State.OR;
                        break;
                    case "PA":
                        ma.Business.LocationAddress.State = State.PA;
                        break;
                    case "RI":
                        ma.Business.LocationAddress.State = State.RI;
                        break;
                    case "SC":
                        ma.Business.LocationAddress.State = State.SC;
                        break;
                    case "SD":
                        ma.Business.LocationAddress.State = State.SD;
                        break;
                    case "TN":
                        ma.Business.LocationAddress.State = State.TN;
                        break;
                    case "TX":
                        ma.Business.LocationAddress.State = State.TX;
                        break;
                    case "UT":
                        ma.Business.LocationAddress.State = State.UT;
                        break;
                    case "VA":
                        ma.Business.LocationAddress.State = State.VA;
                        break;
                    case "VT":
                        ma.Business.LocationAddress.State = State.VT;
                        break;
                    case "WA":
                        ma.Business.LocationAddress.State = State.WA;
                        break;
                    case "WI":
                        ma.Business.LocationAddress.State = State.WI;
                        break;
                    case "WV":
                        ma.Business.LocationAddress.State = State.WV;
                        break;
                    case "WY":
                        ma.Business.LocationAddress.State = State.WY;
                        break;
                }//end switch
                ma.Business.LocationAddress.PostalCode = dr["ZipCode"].ToString().Trim();

                // Initialize the defaults
                ma.Business.AverageMonthlyVolume = Convert.ToDecimal(dr["MonthlyVolume"]);
                ma.Business.AverageSaleAmount = Convert.ToDecimal(dr["AverageTicket"]);
                ma.Business.EstablishmentDate = DateTime.MinValue;
                ma.Business.DBA = dr["DBA"].ToString().Trim();
                ma.Business.FaxNumber = dr["Fax"].ToString().Trim();
                ma.Business.FederalTaxID = dr["FederalTaxID"].ToString().Trim();
                ma.Business.LegalName = dr["CompanyName"].ToString().Trim();
                ma.Business.PhoneNumber = dr["BusinessPhone"].ToString().Trim();
                ma.Business.ProductLine = dr["ProductSold"].ToString().Trim();
                if (dr["LegalStatus"].ToString().Trim() == "Sole Proprietorship")
                    ma.Business.Type = BusinessType.SoleProprietorship;
                else if (dr["LegalStatus"].ToString().Trim() == "Corporation")
                    ma.Business.Type = BusinessType.Corporation;
                else if (dr["LegalStatus"].ToString().Trim() == "Partnership")
                    ma.Business.Type = BusinessType.Partnership;
                else if (dr["LegalStatus"].ToString().Trim() == "Non Profit")
                    ma.Business.Type = BusinessType.NonProfitOrganization;
                else if (dr["LegalStatus"].ToString().Trim() == "LLC")
                    ma.Business.Type = BusinessType.LimitedLiabilityCorporation;
                ma.Business.URL = dr["Website"].ToString().Trim();

                #endregion

                #region CardTypes

                ma.CardTypes[0].CardIssuer = CardIssuer.AmericanExpress;
                if (dr["AmexNum"].ToString().Trim() == "Submitted")
                    ma.CardTypes[0].AcceptAction = CardTypeOptions.Apply;
                else if (dr["AmexNum"].ToString().Trim() == "Opted Out")
                    ma.CardTypes[0].AcceptAction = CardTypeOptions.DontApply;
                else
                {
                    ma.CardTypes[0].AcceptAction = CardTypeOptions.CurrentlyAccept;
                    ma.CardTypes[0].AccountNumber = dr["AmexNum"].ToString().Trim();
                }

                ma.CardTypes[1].CardIssuer = CardIssuer.Diners;
                ma.CardTypes[1].AcceptAction = CardTypeOptions.DontApply;
                ma.CardTypes[1].AccountNumber = "";

                ma.CardTypes[2].CardIssuer = CardIssuer.Discover;
                if ( dr["DiscoverNum"].ToString().Trim() == "Submitted")
                    ma.CardTypes[2].AcceptAction = CardTypeOptions.Apply;
                else if (dr["DiscoverNum"].ToString().Trim() == "Opted Out")
                    ma.CardTypes[2].AcceptAction = CardTypeOptions.DontApply;
                else
                {
                    ma.CardTypes[2].AcceptAction = CardTypeOptions.CurrentlyAccept;
                    ma.CardTypes[2].AccountNumber = dr["DiscoverNum"].ToString().Trim();
                }

                #endregion

                ma.EmailAddress = "jscott@ecenow.com";
                ma.ExternalAgentUserID = "";

                #region Financial
                
                Bank b = ma.Financial.Banks[0];
                b.AccountNumber = dr["CheckingAcctNum"].ToString().Trim();
                b.Address.Address1 = dr["BankAddr"].ToString().Trim();
                b.Address.Address2 = "";
                b.Address.City = dr["BankCity"].ToString().Trim();
                b.Address.PostalCode = dr["BankZip"].ToString().Trim();
                switch (dr["BankState"].ToString().Trim().ToUpper())
                {
                    case "":
                        b.Address.State = State.None;
                        break;
                    case "AK":
                        b.Address.State = State.AK;
                        break;
                    case "AL":
                        b.Address.State = State.AL;
                        break;
                    case "AR":
                        b.Address.State = State.AR;
                        break;
                    case "AZ":
                        b.Address.State = State.AZ;
                        break;
                    case "CA":
                        b.Address.State = State.CA;
                        break;
                    case "CO":
                        b.Address.State = State.CO;
                        break;
                    case "CT":
                        b.Address.State = State.CT;
                        break;
                    case "DC":
                        b.Address.State = State.DC;
                        break;
                    case "DE":
                        b.Address.State = State.DE;
                        break;
                    case "FL":
                        b.Address.State = State.FL;
                        break;
                    case "GA":
                        b.Address.State = State.GA;
                        break;
                    case "HI":
                        b.Address.State = State.HI;
                        break;
                    case "IA":
                        b.Address.State = State.IA;
                        break;
                    case "ID":
                        b.Address.State = State.ID;
                        break;
                    case "IL":
                        b.Address.State = State.IL;
                        break;
                    case "IN":
                        b.Address.State = State.IN;
                        break;
                    case "KS":
                        b.Address.State = State.KS;
                        break;
                    case "KY":
                        b.Address.State = State.KY;
                        break;
                    case "LA":
                        b.Address.State = State.LA;
                        break;
                    case "MA":
                        b.Address.State = State.MA;
                        break;
                    case "MD":
                        b.Address.State = State.MD;
                        break;
                    case "ME":
                        b.Address.State = State.ME;
                        break;
                    case "MI":
                        b.Address.State = State.MI;
                        break;
                    case "MN":
                        b.Address.State = State.MN;
                        break;
                    case "MO":
                        b.Address.State = State.MO;
                        break;
                    case "MS":
                        b.Address.State = State.MS;
                        break;
                    case "MT":
                        b.Address.State = State.MT;
                        break;
                    case "NC":
                        b.Address.State = State.NC;
                        break;
                    case "ND":
                        b.Address.State = State.ND;
                        break;
                    case "NE":
                        b.Address.State = State.NE;
                        break;
                    case "NH":
                        b.Address.State = State.NH;
                        break;
                    case "NJ":
                        b.Address.State = State.NJ;
                        break;
                    case "NM":
                        b.Address.State = State.NM;
                        break;
                    case "NV":
                        b.Address.State = State.NV;
                        break;
                    case "NY":
                        b.Address.State = State.NY;
                        break;
                    case "OH":
                        b.Address.State = State.OH;
                        break;
                    case "OK":
                        b.Address.State = State.OK;
                        break;
                    case "OR":
                        b.Address.State = State.OR;
                        break;
                    case "PA":
                        b.Address.State = State.PA;
                        break;
                    case "RI":
                        b.Address.State = State.RI;
                        break;
                    case "SC":
                        b.Address.State = State.SC;
                        break;
                    case "SD":
                        b.Address.State = State.SD;
                        break;
                    case "TN":
                        b.Address.State = State.TN;
                        break;
                    case "TX":
                        b.Address.State = State.TX;
                        break;
                    case "UT":
                        b.Address.State = State.UT;
                        break;
                    case "VA":
                        b.Address.State = State.VA;
                        break;
                    case "VT":
                        b.Address.State = State.VT;
                        break;
                    case "WA":
                        b.Address.State = State.WA;
                        break;
                    case "WI":
                        b.Address.State = State.WI;
                        break;
                    case "WV":
                        b.Address.State = State.WV;
                        break;
                    case "WY":
                        b.Address.State = State.WY;
                        break;
                }//end switch
                b.Name = dr["BankName"].ToString().Trim();
                b.PhoneNumber = dr["BankPhone"].ToString().Trim();
                b.RoutingNumber = dr["RoutingNum"].ToString().Trim();
                
                ma.Financial.CreditCard.CardholderName = "";
                ma.Financial.CreditCard.AccountNumber = "";
                ma.Financial.CreditCard.ExpirationDate = DateTime.MinValue;

                if (dr["CTMF"].ToString().Trim() == "Yes")
                    ma.History.TerminatedMerchantFile = true;
                else
                    ma.History.TerminatedMerchantFile = false;
                if (dr["PrevProcessed"].ToString().Trim() == "Yes")
                {
                    ma.History.HadPriorCardAquirer = true;
                    ma.History.PriorCardAquirerDepartureReason = dr["ReasonForLeaving"].ToString().Trim();
                    ma.History.PriorCardAquirerName = dr["PrevProcessor"].ToString().Trim();
                }
                else
                    ma.History.HadPriorCardAquirer = false;

                #endregion

                #region Principals
                //Principal #1
                Principal p = ma.Principals[0];
                p.BirthDate = Convert.ToDateTime(dr["P1DOB"]);
                p.ResidenceEstablishmentDate = DateTime.MinValue;
                p.DriverLicenseExpirationDate = Convert.ToDateTime(dr["P1DriversLicenseExp"]);
                p.DriverLicenseNumber = dr["P1DriversLicenseNo"].ToString().Trim();
                dr["P1State"].ToString().Trim();
                switch (dr["P1DriversLicenseState"].ToString().Trim().ToUpper())
                {
                    case "":
                        p.DriverLicenseState = State.None;
                        break;
                    case "AK":
                        p.DriverLicenseState = State.AK;
                        break;
                    case "AL":
                        p.DriverLicenseState = State.AL;
                        break;
                    case "AR":
                        p.DriverLicenseState = State.AR;
                        break;
                    case "AZ":
                        p.DriverLicenseState = State.AZ;
                        break;
                    case "CA":
                        p.DriverLicenseState = State.CA;
                        break;
                    case "CO":
                        p.DriverLicenseState = State.CO;
                        break;
                    case "CT":
                        p.DriverLicenseState = State.CT;
                        break;
                    case "DC":
                        p.DriverLicenseState = State.DC;
                        break;
                    case "DE":
                        p.DriverLicenseState = State.DE;
                        break;
                    case "FL":
                        p.DriverLicenseState = State.FL;
                        break;
                    case "GA":
                        p.DriverLicenseState = State.GA;
                        break;
                    case "HI":
                        p.DriverLicenseState = State.HI;
                        break;
                    case "IA":
                        p.DriverLicenseState = State.IA;
                        break;
                    case "ID":
                        p.DriverLicenseState = State.ID;
                        break;
                    case "IL":
                        p.DriverLicenseState = State.IL;
                        break;
                    case "IN":
                        p.DriverLicenseState = State.IN;
                        break;
                    case "KS":
                        p.DriverLicenseState = State.KS;
                        break;
                    case "KY":
                        p.DriverLicenseState = State.KY;
                        break;
                    case "LA":
                        p.DriverLicenseState = State.LA;
                        break;
                    case "MA":
                        p.DriverLicenseState = State.MA;
                        break;
                    case "MD":
                        p.DriverLicenseState = State.MD;
                        break;
                    case "ME":
                        p.DriverLicenseState = State.ME;
                        break;
                    case "MI":
                        p.DriverLicenseState = State.MI;
                        break;
                    case "MN":
                        p.DriverLicenseState = State.MN;
                        break;
                    case "MO":
                        p.DriverLicenseState = State.MO;
                        break;
                    case "MS":
                        p.DriverLicenseState = State.MS;
                        break;
                    case "MT":
                        p.DriverLicenseState = State.MT;
                        break;
                    case "NC":
                        p.DriverLicenseState = State.NC;
                        break;
                    case "ND":
                        p.DriverLicenseState = State.ND;
                        break;
                    case "NE":
                        p.DriverLicenseState = State.NE;
                        break;
                    case "NH":
                        p.DriverLicenseState = State.NH;
                        break;
                    case "NJ":
                        p.DriverLicenseState = State.NJ;
                        break;
                    case "NM":
                        p.DriverLicenseState = State.NM;
                        break;
                    case "NV":
                        p.DriverLicenseState = State.NV;
                        break;
                    case "NY":
                        p.DriverLicenseState = State.NY;
                        break;
                    case "OH":
                        p.DriverLicenseState = State.OH;
                        break;
                    case "OK":
                        p.DriverLicenseState = State.OK;
                        break;
                    case "OR":
                        p.DriverLicenseState = State.OR;
                        break;
                    case "PA":
                        p.DriverLicenseState = State.PA;
                        break;
                    case "RI":
                        p.DriverLicenseState = State.RI;
                        break;
                    case "SC":
                        p.DriverLicenseState = State.SC;
                        break;
                    case "SD":
                        p.DriverLicenseState = State.SD;
                        break;
                    case "TN":
                        p.DriverLicenseState = State.TN;
                        break;
                    case "TX":
                        p.DriverLicenseState = State.TX;
                        break;
                    case "UT":
                        p.DriverLicenseState = State.UT;
                        break;
                    case "VA":
                        p.DriverLicenseState = State.VA;
                        break;
                    case "VT":
                        p.DriverLicenseState = State.VT;
                        break;
                    case "WA":
                        p.DriverLicenseState = State.WA;
                        break;
                    case "WI":
                        p.DriverLicenseState = State.WI;
                        break;
                    case "WV":
                        p.DriverLicenseState = State.WV;
                        break;
                    case "WY":
                        p.DriverLicenseState = State.WY;
                        break;
                }//end switch
                p.FirstName = dr["P1FirstName"].ToString().Trim();
                p.LastName = dr["P1LastName"].ToString().Trim();
                p.MiddleInitial = dr["P1LastName"].ToString().Trim().Substring(0,1);
                p.OwnershipPercentage = Convert.ToInt32(dr["P1OwnershipPercent"]);
                p.ResidenceAddress.Address1 = dr["P1Address"].ToString().Trim();
                p.ResidenceAddress.Address2 = "";
                p.ResidenceAddress.City = dr["P1City"].ToString();
                p.ResidenceAddress.PostalCode = dr["P1ZipCode"].ToString().Trim();
                switch (dr["P1State"].ToString().Trim().ToUpper())
                {
                    case "":
                        p.ResidenceAddress.State = State.None;
                        break;
                    case "AK":
                        p.ResidenceAddress.State = State.AK;
                        break;
                    case "AL":
                        p.ResidenceAddress.State = State.AL;
                        break;
                    case "AR":
                        p.ResidenceAddress.State = State.AR;
                        break;
                    case "AZ":
                        p.ResidenceAddress.State = State.AZ;
                        break;
                    case "CA":
                        p.ResidenceAddress.State = State.CA;
                        break;
                    case "CO":
                        p.ResidenceAddress.State = State.CO;
                        break;
                    case "CT":
                        p.ResidenceAddress.State = State.CT;
                        break;
                    case "DC":
                        p.ResidenceAddress.State = State.DC;
                        break;
                    case "DE":
                        p.ResidenceAddress.State = State.DE;
                        break;
                    case "FL":
                        p.ResidenceAddress.State = State.FL;
                        break;
                    case "GA":
                        p.ResidenceAddress.State = State.GA;
                        break;
                    case "HI":
                        p.ResidenceAddress.State = State.HI;
                        break;
                    case "IA":
                        p.ResidenceAddress.State = State.IA;
                        break;
                    case "ID":
                        p.ResidenceAddress.State = State.ID;
                        break;
                    case "IL":
                        p.ResidenceAddress.State = State.IL;
                        break;
                    case "IN":
                        p.ResidenceAddress.State = State.IN;
                        break;
                    case "KS":
                        p.ResidenceAddress.State = State.KS;
                        break;
                    case "KY":
                        p.ResidenceAddress.State = State.KY;
                        break;
                    case "LA":
                        p.ResidenceAddress.State = State.LA;
                        break;
                    case "MA":
                        p.ResidenceAddress.State = State.MA;
                        break;
                    case "MD":
                        p.ResidenceAddress.State = State.MD;
                        break;
                    case "ME":
                        p.ResidenceAddress.State = State.ME;
                        break;
                    case "MI":
                        p.ResidenceAddress.State = State.MI;
                        break;
                    case "MN":
                        p.ResidenceAddress.State = State.MN;
                        break;
                    case "MO":
                        p.ResidenceAddress.State = State.MO;
                        break;
                    case "MS":
                        p.ResidenceAddress.State = State.MS;
                        break;
                    case "MT":
                        p.ResidenceAddress.State = State.MT;
                        break;
                    case "NC":
                        p.ResidenceAddress.State = State.NC;
                        break;
                    case "ND":
                        p.ResidenceAddress.State = State.ND;
                        break;
                    case "NE":
                        p.ResidenceAddress.State = State.NE;
                        break;
                    case "NH":
                        p.ResidenceAddress.State = State.NH;
                        break;
                    case "NJ":
                        p.ResidenceAddress.State = State.NJ;
                        break;
                    case "NM":
                        p.ResidenceAddress.State = State.NM;
                        break;
                    case "NV":
                        p.ResidenceAddress.State = State.NV;
                        break;
                    case "NY":
                        p.ResidenceAddress.State = State.NY;
                        break;
                    case "OH":
                        p.ResidenceAddress.State = State.OH;
                        break;
                    case "OK":
                        p.ResidenceAddress.State = State.OK;
                        break;
                    case "OR":
                        p.ResidenceAddress.State = State.OR;
                        break;
                    case "PA":
                        p.ResidenceAddress.State = State.PA;
                        break;
                    case "RI":
                        p.ResidenceAddress.State = State.RI;
                        break;
                    case "SC":
                        p.ResidenceAddress.State = State.SC;
                        break;
                    case "SD":
                        p.ResidenceAddress.State = State.SD;
                        break;
                    case "TN":
                        p.ResidenceAddress.State = State.TN;
                        break;
                    case "TX":
                        p.ResidenceAddress.State = State.TX;
                        break;
                    case "UT":
                        p.ResidenceAddress.State = State.UT;
                        break;
                    case "VA":
                        p.ResidenceAddress.State = State.VA;
                        break;
                    case "VT":
                        p.ResidenceAddress.State = State.VT;
                        break;
                    case "WA":
                        p.ResidenceAddress.State = State.WA;
                        break;
                    case "WI":
                        p.ResidenceAddress.State = State.WI;
                        break;
                    case "WV":
                        p.ResidenceAddress.State = State.WV;
                        break;
                    case "WY":
                        p.ResidenceAddress.State = State.WY;
                        break;
                }//end switch
                if ( dr["P1LivingStatus"].ToString().Trim() == "Rent" )
                    p.ResidenceOwnership = ResidenceOwnership.Rent;
                else
                    p.ResidenceOwnership = ResidenceOwnership.Own;
                p.ResidencePhoneNumber = dr["P1PhoneNumber"].ToString();
                p.SocialSecurityNumber = dr["P1SSN"].ToString();
                p.Suffix = Suffix.None;

                //Principal #2
                Principal p2 = ma.Principals[1];
                if ( dr["P2DOB"].ToString().Trim() != "" )
                    p2.BirthDate = Convert.ToDateTime(dr["P2DOB"]);
                else
                    p2.BirthDate = DateTime.MinValue;
                p2.ResidenceEstablishmentDate = DateTime.MinValue;
                if ( dr["P2DriversLicenseExp"].ToString().Trim() != "" )
                    p2.DriverLicenseExpirationDate = Convert.ToDateTime(dr["P2DriversLicenseExp"]);
                else
                    p2.DriverLicenseExpirationDate = DateTime.MinValue;
                p2.DriverLicenseNumber = dr["P2DriversLicenseNo"].ToString().Trim();
                dr["P2State"].ToString().Trim();
                switch (dr["P2DriversLicenseState"].ToString().Trim().ToUpper())
                {
                    case "":
                        p2.DriverLicenseState = State.None;
                        break;
                    case "AK":
                        p2.DriverLicenseState = State.AK;
                        break;
                    case "AL":
                        p2.DriverLicenseState = State.AL;
                        break;
                    case "AR":
                        p2.DriverLicenseState = State.AR;
                        break;
                    case "AZ":
                        p2.DriverLicenseState = State.AZ;
                        break;
                    case "CA":
                        p2.DriverLicenseState = State.CA;
                        break;
                    case "CO":
                        p2.DriverLicenseState = State.CO;
                        break;
                    case "CT":
                        p2.DriverLicenseState = State.CT;
                        break;
                    case "DC":
                        p2.DriverLicenseState = State.DC;
                        break;
                    case "DE":
                        p2.DriverLicenseState = State.DE;
                        break;
                    case "FL":
                        p2.DriverLicenseState = State.FL;
                        break;
                    case "GA":
                        p2.DriverLicenseState = State.GA;
                        break;
                    case "HI":
                        p2.DriverLicenseState = State.HI;
                        break;
                    case "IA":
                        p2.DriverLicenseState = State.IA;
                        break;
                    case "ID":
                        p2.DriverLicenseState = State.ID;
                        break;
                    case "IL":
                        p2.DriverLicenseState = State.IL;
                        break;
                    case "IN":
                        p2.DriverLicenseState = State.IN;
                        break;
                    case "KS":
                        p2.DriverLicenseState = State.KS;
                        break;
                    case "KY":
                        p2.DriverLicenseState = State.KY;
                        break;
                    case "LA":
                        p2.DriverLicenseState = State.LA;
                        break;
                    case "MA":
                        p2.DriverLicenseState = State.MA;
                        break;
                    case "MD":
                        p2.DriverLicenseState = State.MD;
                        break;
                    case "ME":
                        p2.DriverLicenseState = State.ME;
                        break;
                    case "MI":
                        p2.DriverLicenseState = State.MI;
                        break;
                    case "MN":
                        p2.DriverLicenseState = State.MN;
                        break;
                    case "MO":
                        p2.DriverLicenseState = State.MO;
                        break;
                    case "MS":
                        p2.DriverLicenseState = State.MS;
                        break;
                    case "MT":
                        p2.DriverLicenseState = State.MT;
                        break;
                    case "NC":
                        p2.DriverLicenseState = State.NC;
                        break;
                    case "ND":
                        p2.DriverLicenseState = State.ND;
                        break;
                    case "NE":
                        p2.DriverLicenseState = State.NE;
                        break;
                    case "NH":
                        p2.DriverLicenseState = State.NH;
                        break;
                    case "NJ":
                        p2.DriverLicenseState = State.NJ;
                        break;
                    case "NM":
                        p2.DriverLicenseState = State.NM;
                        break;
                    case "NV":
                        p2.DriverLicenseState = State.NV;
                        break;
                    case "NY":
                        p2.DriverLicenseState = State.NY;
                        break;
                    case "OH":
                        p2.DriverLicenseState = State.OH;
                        break;
                    case "OK":
                        p2.DriverLicenseState = State.OK;
                        break;
                    case "OR":
                        p2.DriverLicenseState = State.OR;
                        break;
                    case "PA":
                        p2.DriverLicenseState = State.PA;
                        break;
                    case "RI":
                        p2.DriverLicenseState = State.RI;
                        break;
                    case "SC":
                        p2.DriverLicenseState = State.SC;
                        break;
                    case "SD":
                        p2.DriverLicenseState = State.SD;
                        break;
                    case "TN":
                        p2.DriverLicenseState = State.TN;
                        break;
                    case "TX":
                        p2.DriverLicenseState = State.TX;
                        break;
                    case "UT":
                        p2.DriverLicenseState = State.UT;
                        break;
                    case "VA":
                        p2.DriverLicenseState = State.VA;
                        break;
                    case "VT":
                        p2.DriverLicenseState = State.VT;
                        break;
                    case "WA":
                        p2.DriverLicenseState = State.WA;
                        break;
                    case "WI":
                        p2.DriverLicenseState = State.WI;
                        break;
                    case "WV":
                        p2.DriverLicenseState = State.WV;
                        break;
                    case "WY":
                        p2.DriverLicenseState = State.WY;
                        break;
                }//end switch
                p2.FirstName = dr["P2FirstName"].ToString().Trim();
                p2.LastName = dr["P2LastName"].ToString().Trim();
                p2.MiddleInitial = "";
                if (dr["P2OwnershipPercent"].ToString().Trim() != "")
                    p2.OwnershipPercentage = Convert.ToInt32(dr["P2OwnershipPercent"]);
                else
                    p2.OwnershipPercentage = 0;
                p2.ResidenceAddress.Address1 = dr["P2Address"].ToString().Trim();
                p2.ResidenceAddress.Address2 = "";
                p2.ResidenceAddress.City = dr["P2City"].ToString();
                p2.ResidenceAddress.PostalCode = dr["P2Zip"].ToString().Trim();
                switch (dr["P2State"].ToString().Trim().ToUpper())
                {
                    case "":
                        p2.ResidenceAddress.State = State.None;
                        break;
                    case "AK":
                        p2.ResidenceAddress.State = State.AK;
                        break;
                    case "AL":
                        p2.ResidenceAddress.State = State.AL;
                        break;
                    case "AR":
                        p2.ResidenceAddress.State = State.AR;
                        break;
                    case "AZ":
                        p2.ResidenceAddress.State = State.AZ;
                        break;
                    case "CA":
                        p2.ResidenceAddress.State = State.CA;
                        break;
                    case "CO":
                        p2.ResidenceAddress.State = State.CO;
                        break;
                    case "CT":
                        p2.ResidenceAddress.State = State.CT;
                        break;
                    case "DC":
                        p2.ResidenceAddress.State = State.DC;
                        break;
                    case "DE":
                        p2.ResidenceAddress.State = State.DE;
                        break;
                    case "FL":
                        p2.ResidenceAddress.State = State.FL;
                        break;
                    case "GA":
                        p2.ResidenceAddress.State = State.GA;
                        break;
                    case "HI":
                        p2.ResidenceAddress.State = State.HI;
                        break;
                    case "IA":
                        p2.ResidenceAddress.State = State.IA;
                        break;
                    case "ID":
                        p2.ResidenceAddress.State = State.ID;
                        break;
                    case "IL":
                        p2.ResidenceAddress.State = State.IL;
                        break;
                    case "IN":
                        p2.ResidenceAddress.State = State.IN;
                        break;
                    case "KS":
                        p2.ResidenceAddress.State = State.KS;
                        break;
                    case "KY":
                        p2.ResidenceAddress.State = State.KY;
                        break;
                    case "LA":
                        p2.ResidenceAddress.State = State.LA;
                        break;
                    case "MA":
                        p2.ResidenceAddress.State = State.MA;
                        break;
                    case "MD":
                        p2.ResidenceAddress.State = State.MD;
                        break;
                    case "ME":
                        p2.ResidenceAddress.State = State.ME;
                        break;
                    case "MI":
                        p2.ResidenceAddress.State = State.MI;
                        break;
                    case "MN":
                        p2.ResidenceAddress.State = State.MN;
                        break;
                    case "MO":
                        p2.ResidenceAddress.State = State.MO;
                        break;
                    case "MS":
                        p2.ResidenceAddress.State = State.MS;
                        break;
                    case "MT":
                        p2.ResidenceAddress.State = State.MT;
                        break;
                    case "NC":
                        p2.ResidenceAddress.State = State.NC;
                        break;
                    case "ND":
                        p2.ResidenceAddress.State = State.ND;
                        break;
                    case "NE":
                        p2.ResidenceAddress.State = State.NE;
                        break;
                    case "NH":
                        p2.ResidenceAddress.State = State.NH;
                        break;
                    case "NJ":
                        p2.ResidenceAddress.State = State.NJ;
                        break;
                    case "NM":
                        p2.ResidenceAddress.State = State.NM;
                        break;
                    case "NV":
                        p2.ResidenceAddress.State = State.NV;
                        break;
                    case "NY":
                        p2.ResidenceAddress.State = State.NY;
                        break;
                    case "OH":
                        p2.ResidenceAddress.State = State.OH;
                        break;
                    case "OK":
                        p2.ResidenceAddress.State = State.OK;
                        break;
                    case "OR":
                        p2.ResidenceAddress.State = State.OR;
                        break;
                    case "PA":
                        p2.ResidenceAddress.State = State.PA;
                        break;
                    case "RI":
                        p2.ResidenceAddress.State = State.RI;
                        break;
                    case "SC":
                        p2.ResidenceAddress.State = State.SC;
                        break;
                    case "SD":
                        p2.ResidenceAddress.State = State.SD;
                        break;
                    case "TN":
                        p2.ResidenceAddress.State = State.TN;
                        break;
                    case "TX":
                        p2.ResidenceAddress.State = State.TX;
                        break;
                    case "UT":
                        p2.ResidenceAddress.State = State.UT;
                        break;
                    case "VA":
                        p2.ResidenceAddress.State = State.VA;
                        break;
                    case "VT":
                        p2.ResidenceAddress.State = State.VT;
                        break;
                    case "WA":
                        p2.ResidenceAddress.State = State.WA;
                        break;
                    case "WI":
                        p2.ResidenceAddress.State = State.WI;
                        break;
                    case "WV":
                        p2.ResidenceAddress.State = State.WV;
                        break;
                    case "WY":
                        p2.ResidenceAddress.State = State.WY;
                        break;
                }//end switch
                if (dr["P2LivingStatus"].ToString().Trim() == "Rent")
                    p2.ResidenceOwnership = ResidenceOwnership.Rent;
                else
                    p2.ResidenceOwnership = ResidenceOwnership.Own;
                p2.ResidencePhoneNumber = dr["P2PhoneNumber"].ToString();
                p2.SocialSecurityNumber = dr["P2SSN"].ToString();
                p2.Suffix = Suffix.None;
                #endregion

                #region OLD CODE
                //****************************************************************************************

                /*
                //Instantiate the MerchantApplication Object
                MerchantApplication ma = InstantiateMerchantApplication(2, 1);
                ma.AgentReferralID = "a913794b-3";
                ma.EmailAddress = "jscott@ecenow.com";

                int i = 0;

                //Instantiate Objects
                //Mail Address
                ma.Business.MailAddress.Address1 = "Address 1";
                ma.Business.MailAddress.Address2 = "Address 2";
                ma.Business.MailAddress.City = "City";
                ma.Business.MailAddress.State = State.AZ;
                ma.Business.MailAddress.PostalCode = "90007";

                //Percentages
                ma.Business.BusinessSectorPercentage.Internet = 0;
                ma.Business.BusinessSectorPercentage.MailPhoneOrder = 0;
                ma.Business.BusinessSectorPercentage.Other = 0;
                ma.Business.BusinessSectorPercentage.Restaurant = 0;
                ma.Business.BusinessSectorPercentage.Retail = 0;
                ma.Business.BusinessSectorPercentage.Service = 100;

                //Location Address
                //ma.Business.LocationAddress = new Address();
                ma.Business.LocationAddress.Address1 = "Location 1";
                ma.Business.LocationAddress.Address2 = "Location 2";
                ma.Business.LocationAddress.City = "LCity";
                ma.Business.LocationAddress.State = State.DC;
                ma.Business.LocationAddress.PostalCode = "90036";

                //Terminal Sector Percentage
                //ma.Business.TerminalSectorPercentage = new TerminalSectorPercentage();
                ma.Business.TerminalSectorPercentage.KeyedWithImprint = 100;
                ma.Business.TerminalSectorPercentage.KeyedWithoutImprint = 0;
                ma.Business.TerminalSectorPercentage.Swiped = 0;

                // Initialize the defaults
                ma.Business.AverageMonthlyVolume = 30;
                ma.Business.AverageSaleAmount = 20;
                ma.Business.EstablishmentDate = DateTime.MinValue;
                ma.Business.DBA = "test";
                ma.Business.FaxNumber = "345-345-3456";
                ma.Business.FederalTaxID = "123123123";
                ma.Business.LegalName = "test";
                ma.Business.PhoneNumber = "123-123-1234";
                ma.Business.ProductLine = "456-456-4567";
                ma.Business.Type = BusinessType.Corporation;
                ma.Business.URL = "www.ecenow.com";
                ma.CardTypes[0].CardIssuer = CardIssuer.AmericanExpress;
                ma.CardTypes[0].AcceptAction = CardTypeOptions.DontApply;
                ma.CardTypes[0].AccountNumber = "";
                ma.CardTypes[1].CardIssuer = CardIssuer.Diners;
                ma.CardTypes[1].AcceptAction = CardTypeOptions.DontApply;
                ma.CardTypes[1].AccountNumber = "";
                ma.CardTypes[2].CardIssuer = CardIssuer.Discover;
                ma.CardTypes[2].AcceptAction = CardTypeOptions.DontApply;
                ma.CardTypes[2].AccountNumber = "";
                ma.EmailAddress = "jscott@ecenow.com";
                ma.ExternalAgentUserID = "";
                for (i = 0; i < ma.Financial.Banks.Length; i++)
                {
                    Bank b = ma.Financial.Banks[i];
                    b.AccountNumber = "45365464555";
                    b.Address.Address1 = "bank address";
                    b.Address.Address2 = "";
                    b.Address.City = "bank city";
                    b.Address.PostalCode = "90054";
                    b.Address.State = State.AK;
                    b.Name = "test bank";
                    b.PhoneNumber = "234-234-2345";
                    b.RoutingNumber = "";
                }//end for banks
                ma.Financial.CreditCard.CardholderName = "";
                ma.Financial.CreditCard.AccountNumber = "";
                ma.Financial.CreditCard.ExpirationDate = DateTime.MinValue;
                ma.History.HadPriorCardAquirer = false;
                ma.History.PriorCardAquirerDepartureReason = "";
                ma.History.PriorCardAquirerName = "";
                ma.History.TerminatedMerchantFile = false;
                for (i = 0; i < ma.Principals.Length; i++)
                {
                    Principal p = ma.Principals[0];
                    p.BirthDate = DateTime.MinValue;
                    p.ResidenceEstablishmentDate = DateTime.MinValue;
                    p.DriverLicenseExpirationDate = DateTime.MinValue;
                    p.DriverLicenseNumber = "4365884635";
                    p.DriverLicenseState = State.AK;
                    p.FirstName = "p1first";
                    p.LastName = "p1last";
                    p.MiddleInitial = "m";
                    p.OwnershipPercentage = 55;
                    p.ResidenceAddress.Address1 = "p1 address1";
                    p.ResidenceAddress.Address2 = "p1 address2";
                    p.ResidenceAddress.City = "p1 city";
                    p.ResidenceAddress.PostalCode = "45565";
                    p.ResidenceAddress.State = State.AK;
                    p.ResidenceOwnership = ResidenceOwnership.Own;
                    p.ResidencePhoneNumber = "123-123-1234";
                    p.SocialSecurityNumber = "111111111";
                    p.Suffix = Suffix.None;
                }//end for principals
                */
                #endregion
                //Instantiate the web service
                MerchantApplicationWebService maws = new MerchantApplicationWebService();

                /*Set the URL for testing.  Note: when ready for implementing
                    the live solution, this is the only line you need to change.
                    Change the MerchantApplicationWebServiceTest.asmx below to
                    read MerchantApplicationWebService.asmx for production.*/
                maws.Url = "https://services.innovativemerchant.com/" +
                    "MerchantApplicationWebService_v1_2/" +
                    "MerchantApplicationWebServiceTest.asmx";

                //Create a UsernameToken object with your username and password
                UsernameToken ut = new UsernameToken("JSCOTT@ECENOW.COM", "487@ECENOW.COM",
                    PasswordOption.SendPlainText);

                //Add the token to the web service        
                maws.RequestSoapContext.Security.Tokens.Add(ut);

                //Declare a result object to capture the
                //result of the web service.
                Result res = null;

                //Make the call to the web service.
                //Handle any exception that may be returned appropriately.
                try
                {                    
                   res = maws.SubmitPartialApplication("MerchantPassword", ma);                    
                }
                catch (Exception ex)
                {

                    /*
                    Server errors should be worked out before releasing your 
                        Application and connection errors should give the user a 
                        generic error. We recommend that you log these errors and 
                        check the log often during testing to ensure that there 
                        are no problems with the post to the web service.
                    */
                    MessageBox.Show(ex.Message);

                }//end catch

                //Display nothing if the object was not returned.  The only        
                //time this will be nothing is when there is an error.      

                if (res != null)
                {
                    //If ValidationErrors is nothing, then the operation was
                    //a success.
                    if (res.ValidationErrors == null)
                    {
                        //If the code makes it here, the merchant should be
                        //redirected to the value in res.RedirectURL.
                        MessageBox.Show(res.ApplicationReferenceID + "; " +
                             res.RedirectURL);
                    }
                    else
                    {
                        //If there are validation errors, these errors should
                        //be displayed to the user to prompt them for the corrections
                        //and omitted data (unless you implement your own validation).
                        //The user should not be redirected until they fix the data
                        //to eliminate these errors.
                        string strErrors = "";
                        //Loop through the ValidationErrors array to retrieve the
                        //text description of each of the errors.
                        for (i = 0; i < res.ValidationErrors.Length; i++)
                        {
                            strErrors += res.ValidationErrors[i].Text +
                                 ControlChars.NewLine;
                        }
                        //Display the validation errors.
                        MessageBox.Show(strErrors);
                    }
                }//end if res not null
            }//end if dataset not null
        }//end submit button click

        private void Main_Load(object sender, EventArgs e)
        {
            MerchantApplicationWebService maws = new MerchantApplicationWebService();
        }//end button click

        
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtEmail.Text.Trim() != "")
                {
                    GetIMSDataSet();
                    grdACTRecords.DataSource = bsGridView;
                }
                else
                    MessageBox.Show("Please enter the Email you want to look up");
            }//end try
            catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        #region Get IMS record based on Email

        public void GetIMSDataSet()
        {
            string strQuery = "Select * from AppSummary Where Email like '%" + txtEmail.Text.Trim() + "%' ";//"Select * from VW_IMSACTPDF where Email like '%" + txtEmail.Text.Trim() + "%'";
            GetIMSInfo(strQuery);
        }

        public void GetIMSInfo(string strQuery)
        {
            string ConnString = "Data Source=CTCSERVER\\ACT7;Persist Security Info=True;Password=Z6x6Y,1ct-QoS6Z41;User ID=SA;Initial Catalog=LAContacts;";
            SqlConnection Conn = new SqlConnection(ConnString);
            try
            {
                SqlCommand cmd = new SqlCommand(strQuery, Conn);
                cmd.Connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataTable dt = new DataTable();
                dt.Locale = System.Globalization.CultureInfo.InvariantCulture;
                adapter.Fill(dt);
                bsGridView.DataSource = dt;                
            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                Conn.Close();
                Conn.Dispose();
            }
        }

        #endregion

        #region Get IMS record based on ContactID

        public DataSet GetIMSRecord(string pContactID)
        {
            string strQuery = "Select * from VW_IMSAPI where ContactID = @ContactID";
            DataSet dsIMS = GetIMSRecordData(strQuery, pContactID);
            return dsIMS;
        }

        public DataSet GetIMSRecordData(string strQuery, string pContactID)
        {
            string ConnString = "Data Source=CTCSERVER\\ACT7;Persist Security Info=True;Password=Z6x6Y,1ct-QoS6Z41;User ID=SA;Initial Catalog=LAContacts;";
            SqlConnection Conn = new SqlConnection(ConnString);
            try
            {
                SqlCommand cmd = new SqlCommand(strQuery, Conn);
                cmd.Parameters.Add( new SqlParameter("@ContactID", pContactID));
                cmd.Connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet ds = new DataSet();
                adapter.Fill(ds, "VW_IMSAPI");
                return ds;
            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                Conn.Close();
                Conn.Dispose();
            }
        }

        #endregion

        private void grdACTRecords_Click(object sender, EventArgs e)
        {            
            
        }

        private void grdACTRecords_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataSet ds = new DataSet();
            int index = e.RowIndex;
            DataGridViewRow grdRow = grdACTRecords.Rows[index];
            string strContactID = Convert.ToString( grdRow.Cells["ContactID"].Value);
            lblContactID.Text = strContactID;
            if (strContactID != "")
                ds = GetIMSRecord(strContactID);
            if (ds.Tables[0].Rows.Count > 0)
            {
                btnSubmit.Visible = true;
                DataRow dr = ds.Tables[0].Rows[0];                
                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    rtbValue.Text = rtbValue.Text + ds.Tables[0].Columns[i].ToString() + " - " + dr[i].ToString().Trim() + System.Environment.NewLine;
                }//end for
            }//end if count not 0
            else
                MessageBox.Show("IMS Data for this Record not found");
        }//end grdview row click
    }
}