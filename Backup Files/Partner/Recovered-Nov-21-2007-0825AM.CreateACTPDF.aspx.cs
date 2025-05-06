using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using BusinessLayer;
using DLPartner;
using System.Text;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web.Mail;

public partial class CreateACTPDF : System.Web.UI.Page
{
    //the selected Contact ID clicked by user
    private static string selContactID = "";
    protected void Page_PreInit(object sender, EventArgs e)
    {
        if (!Session.IsNewSession)
        {
            if (Session.Keys.Count == 0)
                Response.Redirect("../logout.aspx");
            if (User.IsInRole("Employee"))
                Page.MasterPageFile = "Admin.master";
            else if (User.IsInRole("Admin"))
                Page.MasterPageFile = "Admin.master";
        }
    }

    void Page_Init(object sender, EventArgs e)
    {
        ViewStateUserKey = Session.SessionID;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Session.IsNewSession)
        {
            //This page is accessible only by Admins and Employees
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
                Response.Redirect("~/logout.aspx");
        }

        if (Session.IsNewSession)
            Response.Redirect("~/login.aspx");

        if (!Page.IsPostBack)
        {
            if (!User.Identity.IsAuthenticated)
                Response.Redirect("~/login.aspx?Authentication=False");
            txtLookup.Focus();
        }
    }//end page load

    //This function handles grid view button click event
    protected void grdPDF_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            if (e.CommandName == "CreatePDF")//If the CreatePDF button in the grid is clicked
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow grdRow = grdPDF.Rows[index];

                System.Guid ContactID = new Guid(Server.HtmlDecode(grdRow.Cells[1].Text) );
                string strContactID = ContactID.ToString();               
                string Processor = Server.HtmlDecode(grdRow.Cells[7].Text);
                if (strContactID!= "")
                {
                    if (Processor.Contains("IMS"))
                        CreateIMSPDF(strContactID);
                    else if (Processor.ToLower().Contains("ipayment"))
                        CreateIPayPDF(strContactID);
                    else if (Processor.ToLower().Contains("merrick"))
                        CreateMerrickPDF(strContactID);
                    else if (Processor.ToLower().Contains("international"))
                        CreateInternationalPDF(strContactID);
                    else if (Processor.ToLower().Contains("canada"))
                        CreateCanadaPDF(strContactID);
                    else if (Processor.ToLower().Contains("chase"))
                    {
                        //Show the Panel when Creating PDF for Chase
                        selContactID = strContactID;
                        pnlChasePDF.Visible = true;
                        lblDBASel.Text = Server.HtmlDecode(grdRow.Cells[4].Text);
                        lblDBASel.Font.Size = FontUnit.Point(10);
                    }
                    else if (Processor != "")
                        DisplayMessage("Processor " + Processor + " is not a valid Processor for PDF creation.");
                    else
                        DisplayMessage("No Processor assigned to this ACT record. PDF cannot be created.");
                }
            }//end if command name            
            else if (e.CommandName == "CreateXML")//If the Create XML button in the grid is clicked
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow grdRow = grdPDF.Rows[index];
                System.Guid ContactID = new Guid(Server.HtmlDecode(grdRow.Cells[1].Text));

                CreateXML(ContactID);
            }//end if command name            
        }//end try
        catch (Exception err)
        {
            CreateLog Log = new CreateLog();
            Log.ErrorLog(Server.MapPath("~/ErrorLog"), err.Message);
            DisplayMessage(err.Message);
        }
    }//end function grid view button click

    //This function creates the ipayment XML file in the customer folder
    public void CreateXML(System.Guid ContactID)
    {
        try
        {
            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID.ToString());
            IPayXMLBL IPay = new IPayXMLBL();
            PartnerDS.ACTiPayXMLDataTable dt = IPay.GetIPayXML(ContactID);

            if (dt.Rows.Count > 0)
            {

                string FileName = "IPay.xml";
                XmlDocument doc = new XmlDocument();
                doc.Load(Server.MapPath(FileName));//Load the XML file

                #region General Information

                //Get the nodes with the tag AttachedFileName
                //GetElementsByTagName always returns a list of nodes that match the name specified.
                //Even if there is only one node with the tag name, it will return a node list with one item.                
                XmlNodeList nodelist = doc.GetElementsByTagName("AttachedFileName");
                //To access that item, define XMLNode object
                XmlNode node = nodelist.Item(0);
                //This is the text inside the node returned.
                node.InnerText = "";

                nodelist = doc.GetElementsByTagName("BusName");
                node = nodelist.Item(0);
                node.InnerText = dt[0].COMPANYNAME;

                nodelist = doc.GetElementsByTagName("DBA");
                node = nodelist.Item(0);
                node.InnerText = dt[0].DBA;

                #region Business Address
                //Business Address
                nodelist = doc.GetElementsByTagName("BusAddr");
                node = nodelist.Item(0);
                XmlNode childNode = node.ChildNodes.Item(0);
                childNode.InnerText = dt[0].Address;
                childNode = node.ChildNodes.Item(1);
                childNode.InnerText = dt[0].CITY;
                childNode = node.ChildNodes.Item(2);
                childNode.InnerText = dt[0].STATE;
                childNode = node.ChildNodes.Item(3);
                childNode.InnerText = dt[0].Country;
                if (dt[0].ZipCode.Length != 5)
                {
                    DisplayMessage("Business Zip code length must be 5 characters long.");
                    return;
                }
                childNode = node.ChildNodes.Item(4);
                childNode.InnerText = dt[0].ZipCode;
                #endregion

                #region Mail Address
                //Mail Address
                nodelist = doc.GetElementsByTagName("MailAddr");
                node = nodelist.Item(0);
                childNode = node.ChildNodes.Item(0);
                childNode.InnerText = dt[0].BillingAddress;
                childNode = node.ChildNodes.Item(1);
                childNode.InnerText = dt[0].BillingCity;
                childNode = node.ChildNodes.Item(2);
                childNode.InnerText = dt[0].BillingState;
                childNode = node.ChildNodes.Item(3);
                childNode.InnerText = dt[0].billingCountry;
                if (dt[0].BillingZipCode.Length != 5)
                {
                    DisplayMessage("Billing Zip code length must be 5 characters long.");
                    return;
                }
                childNode = node.ChildNodes.Item(4);
                childNode.InnerText = dt[0].BillingZipCode;
                #endregion

                string strLOS = dt[0].LOS;
                if ((strLOS != "0") || (strLOS != ""))
                {
                    if (strLOS.Contains(" "))
                        strLOS = strLOS.Substring(0, strLOS.IndexOf(" "));
                }

                nodelist = doc.GetElementsByTagName("LOS");
                node = nodelist.Item(0);
                node.InnerText = strLOS;

                nodelist = doc.GetElementsByTagName("FedTaxId");
                node = nodelist.Item(0);
                node.InnerText = dt[0].FederalTaxID;

                nodelist = doc.GetElementsByTagName("BusPhone");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BusinessPhone;

                nodelist = doc.GetElementsByTagName("BusFax");
                node = nodelist.Item(0);
                node.InnerText = dt[0].Fax;

                nodelist = doc.GetElementsByTagName("CustPhone");
                node = nodelist.Item(0);
                node.InnerText = dt[0].CustServPhone;

                nodelist = doc.GetElementsByTagName("ContactName");
                node = nodelist.Item(0);
                node.InnerText = dt[0].ContactName;

                nodelist = doc.GetElementsByTagName("NumLocs");
                node = nodelist.Item(0);
                node.InnerText = dt[0].NumberOfLocations;

                nodelist = doc.GetElementsByTagName("LOB");
                node = nodelist.Item(0);
                node.InnerText = dt[0].LOB;

                nodelist = doc.GetElementsByTagName("BusHrs");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BusinessHours;

                nodelist = doc.GetElementsByTagName("email");
                node = nodelist.Item(0);
                node.InnerText = dt[0].Email;

                nodelist = doc.GetElementsByTagName("url");
                node = nodelist.Item(0);
                node.InnerText = dt[0].Website;

                nodelist = doc.GetElementsByTagName("RetailCardSwipe");
                node = nodelist.Item(0);
                node.InnerText = dt[0].ProcessPctSwiped.ToString();

                nodelist = doc.GetElementsByTagName("RetailManuallyKeyed.ToString()");
                node = nodelist.Item(0);
                //node.InnerText = dt[0].ProcessPctKeyed;
                node.InnerText = "";

                nodelist = doc.GetElementsByTagName("Internet.ToString()");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BusinessPctInternet.ToString();

                nodelist = doc.GetElementsByTagName("Moto");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BusinessPctMailOrder.ToString();

                nodelist = doc.GetElementsByTagName("OwnershipType");
                node = nodelist.Item(0);
                node.InnerText = dt[0].LegalStatus;

                nodelist = doc.GetElementsByTagName("CustRefundPol");
                node = nodelist.Item(0);
                node.InnerText = dt[0].RefundPolicy;

                nodelist = doc.GetElementsByTagName("ProdType");
                node = nodelist.Item(0);
                node.InnerText = dt[0].ProductSold;

                nodelist = doc.GetElementsByTagName("DelTime");
                node = nodelist.Item(0);
                node.InnerText = dt[0].NumDaysDelivered;

                nodelist = doc.GetElementsByTagName("Comments");
                node = nodelist.Item(0);
                node.InnerText = dt[0].AddlComments;

                #endregion

                #region Proc History

                nodelist = doc.GetElementsByTagName("EverProcessed");
                node = nodelist.Item(0);
                if (dt[0].PrevProcessed == "Yes")
                {
                    node.InnerText = "1";

                    nodelist = doc.GetElementsByTagName("WhoProcessed");
                    node = nodelist.Item(0);
                    node.InnerText = dt[0].PrevProcessor;

                    nodelist = doc.GetElementsByTagName("FormerMerchNum");
                    node = nodelist.Item(0);
                    node.InnerText = dt[0].PrevMerchantAcctNo;

                    nodelist = doc.GetElementsByTagName("TerminationReason");
                    node = nodelist.Item(0);
                    node.InnerText = dt[0].ReasonForLeaving;
                }
                else
                {
                    node.InnerText = "0";

                    nodelist = doc.GetElementsByTagName("ProcHistory");
                    XmlNodeList childnodelist = doc.GetElementsByTagName("WhoProcessed");
                    node = childnodelist.Item(0);
                    nodelist.Item(0).RemoveChild(node);

                    childnodelist = doc.GetElementsByTagName("FormerMerchNum");
                    node = childnodelist.Item(0);
                    nodelist.Item(0).RemoveChild(node);

                    childnodelist = doc.GetElementsByTagName("TerminationReason");
                    node = childnodelist.Item(0);
                    nodelist.Item(0).RemoveChild(node);

                }

                nodelist = doc.GetElementsByTagName("EverTerminated");
                node = nodelist.Item(0);
                if (dt[0].CTMF == "Yes")
                {
                    node.InnerText = "1";

                    nodelist = doc.GetElementsByTagName("WhoTerminated");
                    node = nodelist.Item(0);
                    node.InnerText = "Processor Name";
                }
                else
                {
                    node.InnerText = "0";
                    nodelist = doc.GetElementsByTagName("ProcHistory");
                    XmlNodeList childnodelist = doc.GetElementsByTagName("WhoTerminated");
                    node = childnodelist.Item(0);
                    nodelist.Item(0).RemoveChild(node);
                }

                #endregion

                #region Principal

                //Principal Information
                nodelist = doc.GetElementsByTagName("Principal");

                #region Principal 1

                //First Principal
                node = nodelist.Item(0);
                childNode = node.ChildNodes.Item(0);
                childNode.InnerText = dt[0].P1FirstName;
                childNode = node.ChildNodes.Item(1);
                childNode.InnerText = dt[0].P1LastName;
                childNode = node.ChildNodes.Item(2);
                childNode.InnerText = dt[0].P1SSN;
                childNode = node.ChildNodes.Item(3);
                childNode.InnerText = dt[0].P1OwnershipPercent;
                childNode = node.ChildNodes.Item(4);
                childNode.InnerText = dt[0].P1Title;

                //P1 Residence Address
                childNode = node.ChildNodes.Item(5);
                XmlNode P1Node = childNode.ChildNodes.Item(0);
                P1Node.InnerText = dt[0].P1Address;
                P1Node = childNode.ChildNodes.Item(1);
                P1Node.InnerText = dt[0].P1City;
                P1Node = childNode.ChildNodes.Item(2);
                P1Node.InnerText = dt[0].P1State;
                P1Node = childNode.ChildNodes.Item(3);
                P1Node.InnerText = dt[0].P1Country;
                if (dt[0].P1ZipCode.Length != 5)
                {
                    DisplayMessage("Principal #1 Zip code length must be 5 characters long.");
                    return;
                }
                P1Node = childNode.ChildNodes.Item(4);
                P1Node.InnerText = dt[0].P1ZipCode;

                childNode = node.ChildNodes.Item(6);
                childNode.InnerText = dt[0].P1LivingStatus;

                string TimeAtAddress = dt[0].P1TimeAtAddress;
                if ((TimeAtAddress != "0") || (TimeAtAddress != ""))
                {
                    if (TimeAtAddress.Contains(" "))
                        TimeAtAddress = TimeAtAddress.Substring(0, TimeAtAddress.IndexOf(" "));
                }

                childNode = node.ChildNodes.Item(7);
                childNode.InnerText = TimeAtAddress;
                childNode = node.ChildNodes.Item(8);
                childNode.InnerText = dt[0].P1PhoneNumber;
                childNode = node.ChildNodes.Item(9);
                childNode.InnerText = dt[0].P1DOB;
                childNode = node.ChildNodes.Item(10);
                childNode.InnerText = dt[0].P1DriversLicenseNo;
                childNode = node.ChildNodes.Item(11);
                childNode.InnerText = dt[0].P1DriversLicenseState;

                #endregion

                #region Principal 2
                //Second Principal
                //Check if second principal name in ACT is not blank, and then create Principal subtree.
                if (dt[0].P2FirstName != "")
                {
                    //Get the root node in the XML file to which the new principal 2 tree will be added.
                    //In this XML, the root node is AppData.
                    XmlNodeList nodelistParent = doc.GetElementsByTagName("AppData");
                    //Create another Principal node <Principal> with the same tag names for principal 1.
                    XmlElement element = doc.CreateElement("Principal");

                    XmlElement childElement = doc.CreateElement("FirstName");
                    childElement.InnerText = dt[0].P2FirstName;
                    //Append the child element to "element", which is the <principal> node created above
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("LastName");
                    childElement.InnerText = dt[0].P2LastName;
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("SSN");
                    childElement.InnerText = dt[0].P2SSN;
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("PercentOwnership");
                    childElement.InnerText = dt[0].P2OwnershipPercent;
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("Title");
                    childElement.InnerText = dt[0].P2Title;
                    element.AppendChild(childElement);

                    #region P2Address
                    //Address
                    XmlElement childElementAddress = doc.CreateElement("Address");

                    childElement = doc.CreateElement("st_name");
                    childElement.InnerText = dt[0].P2Address;
                    childElementAddress.AppendChild(childElement);

                    childElement = doc.CreateElement("city");
                    childElement.InnerText = dt[0].P2City;
                    childElementAddress.AppendChild(childElement);

                    childElement = doc.CreateElement("state");
                    childElement.InnerText = dt[0].P2State;
                    childElementAddress.AppendChild(childElement);

                    childElement = doc.CreateElement("country");
                    childElement.InnerText = dt[0].P2Country;
                    childElementAddress.AppendChild(childElement);

                    if (dt[0].P2ZipCode.Length != 5)
                    {
                        DisplayMessage("Principal #2 Zip code length must be 5 characters long.");
                        return;
                    }

                    childElement = doc.CreateElement("zip");
                    childElement.InnerText = dt[0].P2ZipCode;
                    //Append the ChildElement which holds the stname, city, zip etc to the <Address> node.
                    /*
                     <Address>
                        <St_Name/>
                        <Zip/>
                        <state/>
                        <Country/>
                     </Address>*/
                    childElementAddress.AppendChild(childElement);
                    //Append the <Address> node to the <Principal> node.
                    element.AppendChild(childElementAddress);
                    //End Address
                    #endregion

                    childElement = doc.CreateElement("AddrType");
                    childElement.InnerText = dt[0].P2LivingStatus;
                    element.AppendChild(childElement);

                    TimeAtAddress = dt[0].P2TimeAtAddress;
                    if ((TimeAtAddress != "0") || (TimeAtAddress != ""))
                    {
                        if (TimeAtAddress.Contains(" "))
                            TimeAtAddress = TimeAtAddress.Substring(0, TimeAtAddress.IndexOf(" "));
                    }
                    childElement = doc.CreateElement("LOS");
                    childElement.InnerText = TimeAtAddress;
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("Phone");
                    childElement.InnerText = dt[0].P2PhoneNumber;
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("DOB");
                    childElement.InnerText = dt[0].P2DOB;
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("LicNum");
                    childElement.InnerText = dt[0].P2DriversLicenseNo;
                    element.AppendChild(childElement);

                    childElement = doc.CreateElement("LicState");
                    childElement.InnerText = dt[0].P2DriversLicenseState;
                    element.AppendChild(childElement);

                    node = nodelist.Item(0);//Principal 1 node
                    XmlNode parentNode = nodelistParent.Item(0);//AppData node
                    parentNode.InsertAfter(element, node);//Insert after principal 1 node                    
                }
                #endregion

                #endregion

                #region RepInfo

                nodelist = doc.GetElementsByTagName("RepName");
                node = nodelist.Item(0);
                node.InnerText = dt[0].RepName;

                nodelist = doc.GetElementsByTagName("RepNum");
                node = nodelist.Item(0);
                node.InnerText = dt[0].RepNum;

                #endregion

                #region CardBasedFees

                nodelist = doc.GetElementsByTagName("CardType");
                node = nodelist.Item(0);
                /*if ( dt[0].CardType.ToLower().Contains("visa") )
                    node.InnerText = "4";
                else if (dt[0].CardType.ToLower().Contains("master"))*/
                node.InnerText = "1";

                nodelist = doc.GetElementsByTagName("DiscFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].DiscountRate;

                nodelist = doc.GetElementsByTagName("MidQualFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].DiscRateMidQual;

                nodelist = doc.GetElementsByTagName("NonQualFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].DiscRateNonQual;

                nodelist = doc.GetElementsByTagName("TransFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].TransactionFee;

                nodelist = doc.GetElementsByTagName("AVS");
                node = nodelist.Item(0);
                node.InnerText = dt[0].AVS;

                nodelist = doc.GetElementsByTagName("VoiceAuth");
                node = nodelist.Item(0);
                node.InnerText = dt[0].VoiceAuth;

                nodelist = doc.GetElementsByTagName("ARU");
                node = nodelist.Item(0);
                node.InnerText = "0";//dt[0].ARU;

                #endregion

                #region AgentValues

                nodelist = doc.GetElementsByTagName("MonthlyStatementFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].CustServFee;

                nodelist = doc.GetElementsByTagName("MonthlyMinDiscFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].MonMin;

                nodelist = doc.GetElementsByTagName("ApplicationFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].AppFee;

                nodelist = doc.GetElementsByTagName("SetupFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].AppSetupFee;

                nodelist = doc.GetElementsByTagName("BatchHeaders");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BatchHeader;

                nodelist = doc.GetElementsByTagName("ExternalGWFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].GatewayMonFee;

                nodelist = doc.GetElementsByTagName("WirelessFee");
                node = nodelist.Item(0);
                node.InnerText = dt[0].WirelessAccess;

                #endregion

                #region MiscAgentValues

                nodelist = doc.GetElementsByTagName("MonthlyProcLimit");
                node = nodelist.Item(0);
                node.InnerText = dt[0].MonthlyVolume;

                nodelist = doc.GetElementsByTagName("AvgTicket");
                node = nodelist.Item(0);
                node.InnerText = dt[0].AverageTicket;

                nodelist = doc.GetElementsByTagName("NeedAmex");
                node = nodelist.Item(0);
                node.InnerText = dt[0].AmexApplied;

                nodelist = doc.GetElementsByTagName("NeedDiscover");
                node = nodelist.Item(0);
                node.InnerText = dt[0].DiscoverApplied;

                #endregion

                #region Equipment

                nodelist = doc.GetElementsByTagName("EquipType");
                node = nodelist.Item(0);
                node.InnerText = "G";

                nodelist = doc.GetElementsByTagName("EquipModel");
                node = nodelist.Item(0);
                node.InnerText = "Others";// dt[0].Equipment;

                nodelist = doc.GetElementsByTagName("EquipID");
                node = nodelist.Item(0);
                node.InnerText = "";//dt[0].TerminalID;
                /*}
                else
                {
                    nodelist = doc.GetElementsByTagName("AppData");
                    XmlNodeList childnodelist = doc.GetElementsByTagName("Equipment");
                    node = childnodelist.Item(0);
                    nodelist.Item(0).RemoveChild(node);
                }*/
                #endregion

                #region CardNums

                nodelist = doc.GetElementsByTagName("AMEX");
                //if (dt[0].PrevAmexNum == "")
                //{
                //XmlNodeList childnodelist = doc.GetElementsByTagName("CardNums");
                //childnodelist.Item(0).RemoveChild(nodelist.Item(0));
                //}
                //else
                //{                
                node = nodelist.Item(0);
                node.InnerText = dt[0].PrevAmexNum;
                //}

                if ((dt[0].PrevDiscoverNum.Length != 15) && (dt[0].PrevDiscoverNum != ""))
                    Response.Write("Discover number length not 15. Please check in ACT.");
                nodelist = doc.GetElementsByTagName("Discover");
                node = nodelist.Item(0);
                node.InnerText = dt[0].PrevDiscoverNum;

                nodelist = doc.GetElementsByTagName("Jcb");
                node = nodelist.Item(0);
                node.InnerText = dt[0].PrevJCBNum;

                #endregion

                #region BankInfo

                nodelist = doc.GetElementsByTagName("BankName");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BankName;

                nodelist = doc.GetElementsByTagName("BankAddr");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BankAddress;

                nodelist = doc.GetElementsByTagName("TransroutingNum");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BankRoutingNumber;

                nodelist = doc.GetElementsByTagName("DDA");
                node = nodelist.Item(0);
                node.InnerText = dt[0].BankAccountNumber;

                #endregion




                //doc.Save(Server.MapPath("IPay " + AppId + ".xml"));
                string strFile = "IPay_" + dt[0].ContactName.Replace(" ", "") + "_" + ContactID + ".xml";


                if (FilePath != string.Empty)
                {
                    FilePath = FilePath.ToLower();
                    FilePath = FilePath.Replace("file://s:\\customers", "");
                    FilePath = FilePath.Replace("\\", "/");

                    string strHost = "../../Customers";
                    string strPath = Server.MapPath(strHost + FilePath + "/" + dt[0].P1FirstName.Substring(0, 1) + dt[0].P1LastName + ".xml");
                    doc.Save(strPath);
                }

                //doc.Save(Server.MapPath("../IPayment/IPay_" + dt[0].ContactName.Replace(" ", "") + "_" + ContactID + ".xml"));
                doc.Save(Server.MapPath("../IPayment/IPay_" + dt[0].ContactName.Replace(" ", "") + "_" + ContactID + ".xml"));
    
                //DisplayMessage("XML Created.");
            }//end if count not 0
            else
                DisplayMessage("XML Creation Failed. Ensure this is an iPayment processor and the account is not ACTIVE.");
        }//end try
        catch (Exception err)
        {
            //CreateLog Log = new CreateLog();
            //Log.ErrorLog(Server.MapPath("~/ErrorLog"), err.Message);
            DisplayMessage(err.Message);
        }
    }//end function CreateXML

    //This function handles submit button click event
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        try
        {
            lblError.Visible = false;
            pnlChasePDF.Visible = false;
            if (Page.IsValid)
            {
                grdPDF.Visible = true;
                PDFBL ActRecords = new PDFBL();
                DataSet ds = ActRecords.GetPDFSummaryACT(lstLookup.SelectedItem.Text, txtLookup.Text.Trim());
                if (ds.Tables[0].Rows.Count > 0)
                {
                    grdPDF.DataSource = ds;
                    grdPDF.DataBind();
                }//end if count not 0
                else
                {
                    DisplayMessage("No records found.");
                    grdPDF.Visible = false;
                }
            }//end if page is valid
        }//end try
        catch (Exception err)
        {
            CreateLog Log = new CreateLog();
            Log.ErrorLog(Server.MapPath("~/ErrorLog"), err.Message);
            DisplayMessage("Error retrieving data from ACT!");
        }
    }//end submit button click

    #region IMS PDF
    //This function creates IMS PDF
    public bool CreateIMSPDF(string ContactID)
    {
        //Get data for IMS Application
        PDFBL PDF = new PDFBL();
        PartnerDS.ACTIMSPDFDataTable dt = PDF.GetIMSDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            //Create PDFReader object by passing in the name of PDF to populate
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/IMS Application.pdf"));

            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);//Get the customer path from ACT
            string strPath = "";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                if (P1LastName == "")
                    P1LastName = "Merchant";

                strPath = Server.MapPath(strHost + FilePath + "/" + "IMS_" + P1FirstName.Substring(0, 1) + P1LastName + ".pdf");                
            }
            
            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);
            
            AcroFields acroFields = stamper.AcroFields;

            #region General Information
            acroFields.SetField("app.RepName", dt[0].RepName);
            acroFields.SetField("app.LegalName", dt[0].COMPANYNAME);
            acroFields.SetField("app.DBA", dt[0].DBA);
            acroFields.SetField("app.ApplicantDBA", dt[0].DBA);
            acroFields.SetField("app.Email", dt[0].Email);
            acroFields.SetField("app.ContactName", dt[0].ContactName);
            acroFields.SetField("app.Website", dt[0].Website);
            acroFields.SetField("app.MailingAddress", dt[0].BillingAddress);
            acroFields.SetField("app.MCity", dt[0].BillingCity);
            acroFields.SetField("app.MState", dt[0].BillingState);
            acroFields.SetField("app.MZip", dt[0].BillingZipCode);
            acroFields.SetField("app.LocationAddress", dt[0].Address);
            acroFields.SetField("app.LocationCity", dt[0].CITY);
            acroFields.SetField("app.LocationState", dt[0].STATE);
            acroFields.SetField("app.LZip", dt[0].ZipCode);
            acroFields.SetField("app.Years", dt[0].YearsInBusiness.ToString());
            acroFields.SetField("app.Months", dt[0].MonthsInBusiness.ToString());
            acroFields.SetField("app.FaxNumber", dt[0].Fax);
            acroFields.SetField("app.BusinessPhone", dt[0].BusinessPhone);
            acroFields.SetField("app.TaxID", dt[0].FederalTaxID);
            acroFields.SetField("app.ProductsSold", dt[0].ProductSold);
            if (dt[0].PrevIMSNum != "")
            {
                acroFields.SetField("app.chkIMSMerchant", "Yes");
                acroFields.SetField("app.chkNewMerchant", "No");
            }
            else
            {
                acroFields.SetField("app.chkIMSMerchant", "No");
                acroFields.SetField("app.chkNewMerchant", "Yes");
            }
            acroFields.SetField("app.IMSMerchantNum", dt[0].PrevIMSNum);
            acroFields.SetField("app.PrevProcessor", dt[0].PrevProcessor);
            acroFields.SetField("app.ReasonForLeaving", dt[0].ReasonForLeaving);
            acroFields.SetField("app.OtherRefund", dt[0].OtherRefund);
            if (dt[0].CTMF == "Yes")
            {
                acroFields.SetField("app.chkCTMFYes", "Yes");
                acroFields.SetField("app.chkCTMFNo", "Off");
            }
            else
            {
                acroFields.SetField("app.chkCTMFYes", "Off");
                acroFields.SetField("app.chkCTMFNo", "Yes");
            }

            if (dt[0].PrevProcessed == "Yes")
            {
                acroFields.SetField("app.chkPrevProcessedYes", "Yes");
                acroFields.SetField("app.chkPrevProcessedNo", "Off");
            }
            else
            {
                acroFields.SetField("app.chkPrevProcessedYes", "Off");
                acroFields.SetField("app.chkPrevProcessedNo", "Yes");
            }

            if ((dt[0].RefundPolicy == "Refund within 30 days") || (dt[0].RefundPolicy == "Refund Within 30 Days"))
                acroFields.SetField("app.chkRefund30Days", "Yes");

            if (dt[0].RefundPolicy == "Exchange Only")
                acroFields.SetField("app.chkExchangeOnly", "Yes");

            if (dt[0].RefundPolicy.Contains("Other"))
                acroFields.SetField("app.chkOtherRefund", "Yes");

            if (dt[0].RefundPolicy == "No Refund")
                acroFields.SetField("app.chkNoRefund", "Yes");

            if (dt[0].LegalStatus == "Sole Proprietorship")
                acroFields.SetField("app.chkSoleProp", "Yes");
            if (dt[0].LegalStatus == "Corporation")
                acroFields.SetField("app.chkCorporation", "Yes");
            if (dt[0].LegalStatus == "Partnership")
                acroFields.SetField("app.chkPartnership", "Yes");
            if (dt[0].LegalStatus == "Non-Profit")
                acroFields.SetField("app.chkNonProfit", "Yes");
            if (dt[0].LegalStatus == "LLC")
                acroFields.SetField("app.chkLLC", "Yes");
            #endregion

            #region CardPCT
            acroFields.SetField("app.Swiped.ToString()", dt[0].ProcessPctSwiped.ToString().ToString());
            acroFields.SetField("app.Keyed.ToString()With", dt[0].ProcessPctKeyedwImprint.ToString());
            acroFields.SetField("app.Keyed.ToString()Without", dt[0].ProcessPctKeyedwoImprint.ToString());
            acroFields.SetField("app.Retail", dt[0].BusinessPctRetail.ToString());
            acroFields.SetField("app.Restaurant", dt[0].BusinessPctRestaurant.ToString());
            acroFields.SetField("app.Service", dt[0].BusinessPctService.ToString());
            acroFields.SetField("app.MailPhone", dt[0].BusinessPctMailOrder.ToString().ToString());
            acroFields.SetField("app.Internet.ToString()", dt[0].BusinessPctInternet.ToString().ToString());
            acroFields.SetField("app.Other", dt[0].BusinessPctOther.ToString());
            #endregion

            #region Principal #1
            //Principal #1
            acroFields.SetField("app.P1Zip", dt[0].P1ZipCode);
            acroFields.SetField("app.P1State", dt[0].P1State);
            acroFields.SetField("app.P1City", dt[0].P1City);
            acroFields.SetField("app.P1Address", dt[0].P1Address);
            acroFields.SetField("app.P1Title", dt[0].P1Title);
            acroFields.SetField("app.P1SSN", dt[0].P1SSN);
            acroFields.SetField("app.P1LastName", dt[0].P1LastName);
            acroFields.SetField("app.P1MiddleName", dt[0].P1MName);
            acroFields.SetField("app.P1FirstName", dt[0].P1FirstName);
            acroFields.SetField("app.P1Ownership", dt[0].P1OwnershipPercent.ToString());
            acroFields.SetField("app.P1DOB", dt[0].P1DOB);
            acroFields.SetField("app.P1DriversState", dt[0].P1DriversLicenseState);
            acroFields.SetField("app.P1DriversExp", dt[0].P1DriversLicenseExp);
            acroFields.SetField("app.P1DriversLicenseNo", dt[0].P1DriversLicenseNo);
            acroFields.SetField("app.P1Years", dt[0].P1YearsAtAddress);
            acroFields.SetField("app.P1Months", dt[0].P1MonthsAtAddress);
            acroFields.SetField("app.P1HomePhone", dt[0].P1PhoneNumber);
            if (dt[0].P1LivingStatus == "Rent")
                acroFields.SetField("app.chkP1Rent", "Yes");
            if (dt[0].P1LivingStatus == "Own")
                acroFields.SetField("app.chkP1Own", "Yes");
            #endregion

            #region Principal #2
            //Principal #2
            acroFields.SetField("app.P2Zip", dt[0].P2Zip);
            acroFields.SetField("app.P2State", dt[0].P2State);
            acroFields.SetField("app.P2City", dt[0].P2City);
            acroFields.SetField("app.P2Address", dt[0].p2Address);
            acroFields.SetField("app.P2Title", dt[0].P2Title);
            acroFields.SetField("app.P2SSN", dt[0].P2SSN);
            acroFields.SetField("app.P2LastName", dt[0].P2LastName);
            //acroFields.SetField("app.P2MiddleName", dt[0].P2MidName);
            acroFields.SetField("app.P2FirstName", dt[0].P2FirstName);
            acroFields.SetField("app.P2Ownership", dt[0].P2OwnershipPercent.ToString());
            acroFields.SetField("app.P2DOB", dt[0].P2DOB);
            acroFields.SetField("app.P2DriversState", dt[0].P2DriversLicenseState);
            acroFields.SetField("app.P2DriversExp", dt[0].P2DriversLicenseExp);
            acroFields.SetField("app.P2DriversLicenseNo", dt[0].P2DriversLicenseNo);
            acroFields.SetField("app.P2Years", dt[0].P2YearsAtAddress);
            acroFields.SetField("app.P2Months", dt[0].p2MonthsAtAddress);
            acroFields.SetField("app.P2HomePhone", dt[0].p2PhoneNumber);
            if (dt[0].P2LivingStatus == "Rent")
                acroFields.SetField("app.chkP2Rent", "Yes");
            if (dt[0].P2LivingStatus == "Own")
                acroFields.SetField("app.chkP2Own", "Yes");
            #endregion

            #region Rates
            //Rates
            acroFields.SetField("app.AvgTicket", dt[0].AverageTicket.ToString());
            acroFields.SetField("app.MonthlyVol", dt[0].MonthlyVolume.ToString());
            acroFields.SetField("app.DiscRate", dt[0].DiscountRate.ToString());
            acroFields.SetField("app.TransFee", dt[0].TransactionFee.ToString());
            acroFields.SetField("app.CustServFee", dt[0].CustServFee.ToString());
            acroFields.SetField("app.MonMin", dt[0].MonMin.ToString());
            acroFields.SetField("app.SoftwareType", dt[0].Gateway);
            acroFields.SetField("app.TerminalType", dt[0].TerminalType);
            acroFields.SetField("app.TerminalModel", dt[0].TerminalModel);
            acroFields.SetField("app.CGDiscRate", dt[0].CGDiscRate.ToString());
            acroFields.SetField("app.CGTransFee", dt[0].CGTransFee.ToString());
            acroFields.SetField("app.DebitTransFee", dt[0].DebitTransFee.ToString());

            acroFields.SetField("app.MidQualStep", dt[0].DiscRateMidQualStep.ToString());
            acroFields.SetField("app.NonQualStep", dt[0].DiscRateNonQualStep.ToString());
            //acroFields.SetField("app.GatewayTransFee", dt[0].GatewayTransFee.ToString());
            #endregion

            #region Banking
            //Banking
            if (dt[0].DiscoverAccepted == "1")
            {
                acroFields.SetField("app.DiscoverAcctNumbers", dt[0].PrevDiscoverNum);
                acroFields.SetField("app.chkDiscover", "Yes");
            }
            if (dt[0].AmexAccepted == "1")
            {
                acroFields.SetField("app.AmexAcctNumbers", dt[0].PrevAmexNum);
                acroFields.SetField("app.chkAmex", "Yes");
            }
            if (dt[0].jcbAccepted == "1")
            {
                acroFields.SetField("app.JCBAcctNumbers", dt[0].PrevJCBNum);
                acroFields.SetField("app.chkJCB", "Yes");
            }
            acroFields.SetField("app.NBCTransFee", dt[0].NBCTransFee.ToString());
            acroFields.SetField("app.ApplicationFee", "0.00");
            #endregion

            stamper.FormFlattening = true;
            stamper.Close();
            //Response.OutputStream.Write(mStream.GetBuffer(), 0, mStream.GetBuffer().Length - 1);
            //Response.Flush();
            //Response.Close();

            DisplayMessage("PDF created in the customer folder - " + FilePath);
            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("IMS Data not found for this record.");
            return false;
        }
    }//end function CreateIMSPDF
    #endregion

    #region IPAYMENT PDF
    //This function creates iPayment PDF
    public bool CreateIPayPDF(string ContactID)
    {
        //Get data for IPayment Application
        PDFBL IPayData = new PDFBL();
        PartnerDS.ACTiPayPDFDataTable dt = IPayData.GetIPayDataFromACT(ContactID);

        if (dt.Rows.Count > 0)
        {
            //Populate data in PDF
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/ipayment application.pdf"));

            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                if (P1LastName == "")
                    P1LastName = "Merchant";

                strPath = Server.MapPath(strHost + FilePath + "/" + "iPayment_" + P1FirstName.Substring(0, 1) + P1LastName + ".pdf");
            }
            //MemoryStream mStream = new MemoryStream();
            //PdfStamper stamper = new PdfStamper(reader, mStream);
            //stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            AcroFields acroFields = stamper.AcroFields;
            
            #region General Information
            acroFields.SetField("Representative Name", dt[0].RepName);
            acroFields.SetField("Rep #", dt[0].RepNum);
            acroFields.SetField("Office #", "248");
            acroFields.SetField("Rep's phone #", dt[0].RepPhone);
            acroFields.SetField("Legal Name", dt[0].COMPANYNAME);
            acroFields.SetField("DBA", dt[0].DBA);
            acroFields.SetField("Business Address", dt[0].Address);
            acroFields.SetField("City / State", dt[0].CITY + ", " + dt[0].STATE + ", " + dt[0].ZipCode);
            acroFields.SetField("How Long", dt[0].TABL);
            acroFields.SetField("Mailing Address", dt[0].BillingAddress);                      
            acroFields.SetField("M City / State", dt[0].BillingCity + ", " + dt[0].BillingState + ", " + dt[0].BillingZipCode);
            acroFields.SetField("Federal Tax ID", dt[0].FederalTaxID);
            acroFields.SetField("Business Phone", dt[0].BusinessPhone);
            acroFields.SetField("Customer Service Phone", dt[0].CustServPhone);
            acroFields.SetField("Fax #", dt[0].Fax);
            acroFields.SetField("Contact Name", dt[0].ContactName);
            acroFields.SetField("# of Locations", dt[0].NumberOfLocations);
            acroFields.SetField("Time in business", dt[0].YIB.ToString());
            acroFields.SetField("Time in Business", dt[0].MIB.ToString());
            acroFields.SetField("Business Hours", dt[0].BusinessHours);
            
            acroFields.SetField("EMail Address ", dt[0].Email);
            acroFields.SetField("Business Website", dt[0].Website);

            #region CardPCT
            acroFields.SetField("Retail Swipe", dt[0].ProcessPctSwiped.ToString());
            acroFields.SetField("Retail Keyed.ToString()", dt[0].ProcessPctKeyed.ToString());
            acroFields.SetField("Mail Order.ToString()", dt[0].BusinessPctMailOrder.ToString());
            acroFields.SetField("Internet.ToString()", dt[0].BusinessPctInternet.ToString());
            #endregion

            acroFields.SetField("Product type/sold", dt[0].ProductSold);
            acroFields.SetField("Previous Processor", dt[0].PrevProcessor);
            acroFields.SetField("Former Merchant #", dt[0].PrevMerchantAcctNo);

            #region RefundPolicy
            if ((dt[0].RefundPolicy == "Refund within 30 days") || (dt[0].RefundPolicy == "Refund Within 30 Days"))
                acroFields.SetField("Check Box80", "Yes");

            if (dt[0].RefundPolicy == "Exchange Only")
                acroFields.SetField("Check Box81", "Yes");

            if (dt[0].RefundPolicy == "None")
                acroFields.SetField("Check Box82", "Yes");

            if (dt[0].RefundPolicy.Contains("Other"))
                acroFields.SetField("Check Box83", "Yes");
            acroFields.SetField("Customer Return Policy", dt[0].OtherRefund);
            #endregion

            //acroFields.SetField("AddlComments", dt[0].AddlComments);
            //acroFields.SetField("BusinessPhoneExt", dt[0].BusinessPhoneExt);
            acroFields.SetField("# of days product delivered", dt[0].NumDaysDel);

            if (dt[0].CTMF == "Yes")
            {
                acroFields.SetField("Check Box94", "Yes");
                acroFields.SetField("Check Box95", "Off");
            }
            else
            {
                acroFields.SetField("Check Box94", "Off");
                acroFields.SetField("Check Box95", "Yes");
            }

            if (dt[0].PrevProcessed == "Yes")
            {
                acroFields.SetField("Check Box90", "Yes");
                acroFields.SetField("Check Box91", "Off");
            }
            else
            {
                acroFields.SetField("Check Box90", "Off");
                acroFields.SetField("Check Box91", "Yes");
            }

            //if (dt[0].Reprogram == "Yes")
            //acroFields.SetField("chkReprogram", "Yes");
         
            if (dt[0].LegalStatus == "Sole Proprietorship")
                acroFields.SetField("Check Box43", "Yes");
            if (dt[0].LegalStatus == "Corporation")
                acroFields.SetField("Check Box44", "Yes");
            if (dt[0].LegalStatus == "Partnership")
                acroFields.SetField("Check Box47", "Yes");
            if (dt[0].LegalStatus == "Non-Profit")
                acroFields.SetField("Check Box48", "Yes");
            if (dt[0].LegalStatus == "Legal/Medical Corp.")
                acroFields.SetField("Check Box52", "Yes");
            if (dt[0].LegalStatus == "Government")
                acroFields.SetField("Check Box49", "Yes");
            if (dt[0].LegalStatus == "Tax Exempt")
                acroFields.SetField("Check Box50", "Yes");
            if (dt[0].LegalStatus == "Other")
                acroFields.SetField("Check Box46", "Yes");
            if (dt[0].LegalStatus == "LLC")
                acroFields.SetField("Check Box45", "Yes");

            if (dt[0].Equipment != "")
            {
                string equipment = dt[0].Equipment;           
                if (equipment.Contains("Hypercom"))
                    acroFields.SetField("Check Box135", "Yes");            
                else if (equipment.Contains("Verifone"))
                    acroFields.SetField("Check Box136", "Yes");
                else if (equipment.Contains("Nurit"))
                    acroFields.SetField("Check Box137", "Yes");
                else
                {
                    acroFields.SetField("Check Box138", "Yes");
                    acroFields.SetField("Other manufacturer", equipment);
                }
                acroFields.SetField("Terminal Model", equipment);
            }

            #endregion          

            #region Principal #1
            //Principal #1
            acroFields.SetField("Principal 1 Name", dt[0].P1FirstName + " " + dt[0].P1LastName);
            acroFields.SetField("P1 SS Number", dt[0].P1SSN);
            acroFields.SetField("P1 ownership %", dt[0].P1OwnershipPercent.ToString());
            acroFields.SetField("P1 Title", dt[0].P1Title);
            acroFields.SetField("P1 Address", dt[0].P1Address);
            acroFields.SetField("P1 City", dt[0].P1City);
            acroFields.SetField("P1 State", dt[0].P1State);          
            acroFields.SetField("P1 Zip", dt[0].P1ZipCode);
            acroFields.SetField("How long at address", dt[0].P1TimeAtAddress);
            acroFields.SetField("Home Telephone", dt[0].P1PhoneNumber);        
            acroFields.SetField("P1 Date of Birth", dt[0].P1DOB);
            acroFields.SetField("P1 License # and State",  dt[0].P1DriversLicenseNo + " " + dt[0].P1DriversLicenseState);
            
            if (dt[0].P1LivingStatus == "Rent")
                acroFields.SetField("Check Box104", "Yes");
            if (dt[0].P1LivingStatus == "Own")
                acroFields.SetField("Check Box105", "Yes");
            #endregion

            #region Principal #2
            //Principal #2
            acroFields.SetField("Principal 2 name", dt[0].P2FirstName + " " + dt[0].P2LastName);
            acroFields.SetField("P2 SSN", dt[0].P2SSN);
            acroFields.SetField("P2 ownership %", dt[0].P2OwnershipPercent.ToString());
            acroFields.SetField("P2 Title", dt[0].P2Title);
            acroFields.SetField("P2 Address", dt[0].p2Address);
            acroFields.SetField("P2 City", dt[0].P2City);
            acroFields.SetField("P2 State", dt[0].P2State);          
            acroFields.SetField("P2 Zip", dt[0].P2ZipCode);
            //acroFields.SetField("P2 How long at address", dt[0].P2TimeAtAddress);
          
            acroFields.SetField("P2DOB", dt[0].P2DOB);
            acroFields.SetField("P2 Drivers License and State",dt[0].P2DriversLicenseNo + " " + dt[0].P2DriversLicenseState);
            acroFields.SetField("P2 Home Telephone", dt[0].p2PhoneNumber);
            
            if (dt[0].P2LivingStatus == "Rent")
                acroFields.SetField("Check Box118", "Yes");
            if (dt[0].P2LivingStatus == "Own")
                acroFields.SetField("Check Box119", "Yes");
            #endregion

            #region Rates
            //Rates
            acroFields.SetField("Average Ticket", dt[0].AverageTicket.ToString());
            acroFields.SetField("Monthly Sales Processing Limit", dt[0].MonthlyVolume.ToString());
            acroFields.SetField("V/MC Transaction Fee", dt[0].TransactionFee.ToString());
        
            //acroFields.SetField("Customer Service Fee", dt[0].CustServFee.ToString());
            //acroFields.SetField("RetrievalRequest", dt[0].RetrievalFee.ToString());
            //acroFields.SetField("ChargeBacks", dt[0].ChargebackFee.ToString());
            acroFields.SetField("Application Fee", dt[0].AppFee.ToString());
            //acroFields.SetField("SetupFee", dt[0].AppSetupFee.ToString());
            //acroFields.SetField("AVS", dt[0].AVS.ToString());
            //acroFields.SetField("BatchHeader", dt[0].BatchHeader);
            //acroFields.SetField("VoiceAuth", dt[0].VoiceAuth.ToString());
            //acroFields.SetField("BatchHeader", dt[0].BatchHeader);
            acroFields.SetField("Monthly Wireeless Fee", dt[0].WirelessAccessFee.ToString());
            acroFields.SetField("Wireless Auth Fee", dt[0].WirelessTransFee.ToString());
            acroFields.SetField("PIN Debit Card Fee", dt[0].DebitMonFee.ToString());
            acroFields.SetField("PIN Debit Card Trans Fee", dt[0].DebitTransFee.ToString());

            //If Restaurant percentage is ZERO or BLANK
            if ((dt[0].PctRest.ToString() == "") || (Convert.ToDouble(dt[0].PctRest.ToString()) == 0))
            {
                acroFields.SetField("Credit Card Qualified Fee", dt[0].DiscountRate.ToString());
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateMidQual.ToString() != ""))
                    acroFields.SetField("Credit Card Mid Qualified Fee", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateMidQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString())));
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateNonQual.ToString() != ""))
                    acroFields.SetField("Credit Card Non Qualified Fee", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateNonQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString())));
         
            }
            else //Restaurant Pct is greater than ZERO              
            {
                acroFields.SetField("Restaurant/Lodging Rate", dt[0].DiscountRate.ToString());
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateMidQual.ToString() != ""))
                    acroFields.SetField("Restaurant/Lodging Mid Qualified", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateMidQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString())));
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateNonQual.ToString() != ""))
                    acroFields.SetField("Restaurant/Lodging Non Qualified", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateNonQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString())));            
            }
            if (dt[0].DiscRateQualDebit.ToString() != "") 
            {
                acroFields.SetField("Debit Card Qualified Rate", dt[0].DiscRateQualDebit.ToString());
                if (dt[0].DiscRateMidQual.ToString() != "")
                {
                    acroFields.SetField("Debit Card Qualified Mid Qual", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateMidQual) - Convert.ToDecimal(dt[0].DiscRateQualDebit)));
                    acroFields.SetField("Debit Card Qualified Non Qual", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateNonQual) - Convert.ToDecimal(dt[0].DiscRateQualDebit)));
                }
            }

            acroFields.SetField("Gateway", dt[0].Gateway);
            acroFields.SetField("Monthly Internet.ToString() Access Fee", dt[0].GatewayMonFee.ToString());
            acroFields.SetField("Internet.ToString() Gateway Per-Auth Fee", dt[0].GatewayTransFee.ToString());
            #endregion

            #region Banking
            //Baking
            acroFields.SetField("Discover Card Existing Number", dt[0].PrevDiscoverNum);
            acroFields.SetField("Amex Existing Number", dt[0].PrevAmexNum);
            acroFields.SetField("JCB Existing Number", dt[0].PrevJCBNum);
            acroFields.SetField("Bank Name", dt[0].BankName);
            acroFields.SetField("Bank Address", dt[0].BankAddress);
            acroFields.SetField("City", dt[0].BankCity);
            acroFields.SetField("State", dt[0].BankState);
            acroFields.SetField("Zip Code", dt[0].BankZip);
            acroFields.SetField("Bank Telephone Number", dt[0].BankPhone);
            acroFields.SetField("Transit Routing #", dt[0].BankRoutingNumber);
            acroFields.SetField("Checking Account #", dt[0].BankAccountNumber);
            //acroFields.SetField("BankContactName", dt[0].NameOnCheckingAcct);
            #endregion

            #region Platform
            if (dt[0].Platform.ToString().Contains("Omaha") )
                acroFields.SetField("Check Box163", "Yes");
            else if (dt[0].Platform.ToString().Contains ("Nashville") )
                acroFields.SetField("Check Box165", "Yes");
            else if (dt[0].Platform.ToString().Contains ("Vital") )
                acroFields.SetField("Check Box166", "Yes");
            else if (dt[0].Platform.ToString().Contains ("North") )
                acroFields.SetField("Check Box164", "Yes");
            else if ((dt[0].Platform != "") && (dt[0].Platform.ToLower() != "none"))
            {
                acroFields.SetField("Check Box167", "Yes");
                acroFields.SetField("Other Platform", dt[0].Platform);
            }

            //if record has equipment and a gateway
           
            #endregion
            
            /*Code for Previous PDF Version
            #region General Information
            acroFields.SetField("app.RepName", dt[0].RepName);
            acroFields.SetField("app.LegalName", dt[0].COMPANYNAME);
            acroFields.SetField("app.DBA", dt[0].DBA);
            acroFields.SetField("app.ApplicantDBA", dt[0].DBA);
            acroFields.SetField("app.EMail", dt[0].Email);
            acroFields.SetField("app.ContactName", dt[0].ContactName);
            acroFields.SetField("app.Website", dt[0].Website);
            acroFields.SetField("app.MailingAddress", dt[0].BillingAddress);
            acroFields.SetField("app.MCityState", dt[0].BillingCity + ", " + dt[0].BillingState + ", " + dt[0].BillingZipCode);
            acroFields.SetField("app.BusinessAddress", dt[0].Address);
            acroFields.SetField("app.CityState", dt[0].CITY + ", " + dt[0].STATE + ", " + dt[0].ZipCode);
            acroFields.SetField("app.HowLong", dt[0].TABL);
            acroFields.SetField("app.TIBYears", dt[0].YIB.ToString() );
            acroFields.SetField("app.TIBMonths", dt[0].MIB.ToString());
            acroFields.SetField("app.Fax", dt[0].Fax);
            acroFields.SetField("app.BusinessPhone", dt[0].BusinessPhone);
            acroFields.SetField("app.CustServPhone", dt[0].CustServPhone);
            acroFields.SetField("app.BusinessHours", dt[0].BusinessHours);
            acroFields.SetField("app.FederalTaxID", dt[0].FederalTaxID);
            acroFields.SetField("app.ProductsSold", dt[0].ProductSold);
            acroFields.SetField("app.PrevProcessor", dt[0].PrevProcessor);
            acroFields.SetField("app.PrevMerchantNum", dt[0].PrevMerchantAcctNo);
            acroFields.SetField("app.RepNum", dt[0].RepNum);
            acroFields.SetField("app.RepPhone", dt[0].RepPhone);
           acroFields.SetField("app.AddlComments", dt[0].AddlComments);
            acroFields.SetField("app.NumLocs", dt[0].NumberOfLocations);
            acroFields.SetField("app.BusinessPhoneExt", dt[0].BusinessPhoneExt);
            acroFields.SetField("app.NumDaysDel", dt[0].NumDaysDelivered);

            if (dt[0].CTMF == "Yes")
            {
                acroFields.SetField("app.chkCTMFYes", "Yes");
                acroFields.SetField("app.chkCTMFNo", "Off");
            }
            else
            {
                acroFields.SetField("app.chkCTMFYes", "Off");
                acroFields.SetField("app.chkCTMFNo", "Yes");
            }

            if (dt[0].PrevProcessed == "Yes")
            {
                acroFields.SetField("app.chkPrevProcessedYes", "Yes");
                acroFields.SetField("app.chkPrevProcessedNo", "Off");
            }
            else
            {
                acroFields.SetField("app.chkPrevProcessedYes", "Off");
                acroFields.SetField("app.chkPrevProcessedNo", "Yes");
            }

            //if (dt[0].Reprogram == "Yes")
            //acroFields.SetField("app.chkReprogram", "Yes");

            if ((dt[0].RefundPolicy == "Refund within 30 days") || (dt[0].RefundPolicy == "Refund Within 30 Days"))
                acroFields.SetField("app.chkRefund30Days", "Yes");
            else if (dt[0].RefundPolicy == "Exchange Only")
                acroFields.SetField("app.chkExchangeOnly", "Yes");
            else if (dt[0].RefundPolicy == "No Refund")
            {
                acroFields.SetField("app.chkRefundOther", "Yes");
                acroFields.SetField("app.OtherRefund", "No Refund");       
            }
            else if (dt[0].RefundPolicy.Contains("Other"))
            {
                acroFields.SetField("app.chkRefundOther", "Yes");
                acroFields.SetField("app.OtherRefund", dt[0].OtherRefund);       
            }

         

            if (dt[0].LegalStatus == "Sole Proprietorship")
                acroFields.SetField("app.chkSole", "Yes");
            if (dt[0].LegalStatus == "Corporation")
                acroFields.SetField("app.chkCorp", "Yes");
            if (dt[0].LegalStatus == "Partnership")
                acroFields.SetField("app.chkPartnership", "Yes");
            if (dt[0].LegalStatus == "Non-Profit")
                acroFields.SetField("app.chkNonProfit", "Yes");
            if (dt[0].LegalStatus == "Legal/Medical Corp.")
                acroFields.SetField("app.chkLegaMedical", "Yes");
            if (dt[0].LegalStatus == "Government")
                acroFields.SetField("app.chkGovt", "Yes");
            if (dt[0].LegalStatus == "Tax Exempt")
                acroFields.SetField("app.chkTaxExempt", "Yes");
            if (dt[0].LegalStatus == "Others")
                acroFields.SetField("app.chkOwnershipOther", "Yes");
            if (dt[0].LegalStatus == "LLC")
                acroFields.SetField("app.chkLLC", "Yes");

            if (dt[0].Equipment != "")
            {
                string equipment = dt[0].Equipment;
                acroFields.SetField("app.EquipModel", equipment);
                if (equipment.Contains("Nurit"))
                    acroFields.SetField("app.chkNurit", "Yes");
                else if (equipment.Contains("Verifone"))
                    acroFields.SetField("app.chkVerifone", "Yes");
                else if (equipment.Contains("Hypercom"))
                    acroFields.SetField("app.chkHypercom", "Yes");
                else
                    acroFields.SetField("app.chkOther", "Yes");

            }

            #endregion

            #region CardPCT
            acroFields.SetField("app.Swiped.ToString()", dt[0].ProcessPctSwiped.ToString().ToString());
            acroFields.SetField("app.Keyed.ToString()", dt[0].ProcessPctKeyed.ToString().ToString());
            acroFields.SetField("app.MailOrder.ToString()", dt[0].BusinessPctMailOrder.ToString().ToString());
            acroFields.SetField("app.Internet.ToString()", dt[0].BusinessPctInternet.ToString().ToString());
            #endregion

            #region Principal #1
            //Principal #1
            acroFields.SetField("app.P1ZipCode", dt[0].P1ZipCode);
            acroFields.SetField("app.P1State", dt[0].P1State);
            acroFields.SetField("app.P1City", dt[0].P1City);
            acroFields.SetField("app.P1Address", dt[0].P1Address);
            acroFields.SetField("app.P1Title", dt[0].P1Title);
            acroFields.SetField("app.P1SSN", dt[0].P1SSN);
            acroFields.SetField("app.P1Name", dt[0].P1FirstName + " " + dt[0].P1LastName);
            acroFields.SetField("app.P1Ownership", dt[0].P1OwnershipPercent.ToString());
            acroFields.SetField("app.P1DOB", dt[0].P1DOB);
            acroFields.SetField("app.P1DState", dt[0].P1DriversLicenseState);
            acroFields.SetField("app.P1DriversLicense", dt[0].P1DriversLicenseNo);
            acroFields.SetField("app.P1HomePhone", dt[0].P1PhoneNumber);
            acroFields.SetField("app.P1TimeAtAddress", dt[0].P1TimeAtAddress);
            if (dt[0].P1LivingStatus == "Rent")
                acroFields.SetField("app.chkP1Rent", "Yes");
            if (dt[0].P1LivingStatus == "Own")
                acroFields.SetField("app.chkP1Own", "Yes");
            #endregion

            #region Principal #2
            //Principal #2
            acroFields.SetField("app.P2ZipCode", dt[0].P2ZipCode);
            acroFields.SetField("app.P2State", dt[0].P2State);
            acroFields.SetField("app.P2City", dt[0].P2City);
            acroFields.SetField("app.P2Address", dt[0].p2Address);
            acroFields.SetField("app.P2Title", dt[0].P2Title);
            acroFields.SetField("app.P2SSN", dt[0].P2SSN);
            acroFields.SetField("app.P2Name", dt[0].P2FirstName + " " + dt[0].P2LastName);
            acroFields.SetField("app.P2Ownership", dt[0].P2OwnershipPercent.ToString());
            acroFields.SetField("app.P2DOB", dt[0].P2DOB);
            acroFields.SetField("app.P2DState", dt[0].P2DriversLicenseState);
            acroFields.SetField("app.P2DriversLicense", dt[0].P2DriversLicenseNo);
            acroFields.SetField("app.P2HomePhone", dt[0].p2PhoneNumber);
            acroFields.SetField("app.P2TimeAtAddress", dt[0].P2TimeAtAddress);
            if (dt[0].P2LivingStatus == "Rent")
                acroFields.SetField("app.chkP2Rent", "Yes");
            if (dt[0].P2LivingStatus == "Own")
                acroFields.SetField("app.chkP2Own", "Yes");
            #endregion

            #region Rates
            //Rates
            acroFields.SetField("app.AvgTicket", dt[0].AverageTicket.ToString());
            acroFields.SetField("app.MonthlySalesProcessingLimit", dt[0].MonthlyVolume.ToString());
            acroFields.SetField("app.TransactionFee", dt[0].TransactionFee.ToString());
            acroFields.SetField("app.AnnualFeeCP", dt[0].AnnualFeeCP.ToString());
            acroFields.SetField("app.AnnualFeeCNP", dt[0].AnnualFeeCNP.ToString());
            acroFields.SetField("app.CustServFee", dt[0].CustServFee.ToString());
            acroFields.SetField("app.MonthlyMinDiscFee", dt[0].MonMin.ToString());
            acroFields.SetField("app.RetrievalRequest", dt[0].RetrievalFee.ToString());
            acroFields.SetField("app.ChargeBacks", dt[0].ChargebackFee.ToString());
            acroFields.SetField("app.ApplicationFee", dt[0].AppFee.ToString());
            acroFields.SetField("app.SetupFee", dt[0].AppSetupFee.ToString());
            acroFields.SetField("app.AVS", dt[0].AVS.ToString());
            acroFields.SetField("app.BatchHeader", dt[0].BatchHeader.ToString());
            acroFields.SetField("app.VoiceAuth", dt[0].VoiceAuth.ToString().ToString());
            acroFields.SetField("app.BatchHeader", dt[0].BatchHeader.ToString());
            acroFields.SetField("app.WirelessMonthlyGatewayFee", dt[0].WirelessAccessFee.ToString());
            acroFields.SetField("app.WirelessPerAuthFee", dt[0].WirelessTransFee.ToString());
            acroFields.SetField("app.DebitCardAccessFee", dt[0].DebitMonFee.ToString());
            acroFields.SetField("app.Debit", dt[0].DebitTransFee.ToString());

            //If Restaurant percentage is ZERO or BLANK
            if ((dt[0].PctRest.ToString() == "") || (Convert.ToInt16(dt[0].PctRest) == 0))
            {
                acroFields.SetField("app.QualifiedFee", dt[0].DiscountRate.ToString());
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateMidQual.ToString().ToString() != ""))
                    acroFields.SetField("app.MidQualifiedFee", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateMidQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString() ) ) );
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateNonQual.ToString() != ""))
                    acroFields.SetField("app.NonQualifiedFee", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateNonQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString())));

            }
            else //Restaurant Pct is greater than ZERO              
            {
                acroFields.SetField("app.QualFee", dt[0].DiscountRate.ToString());
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateMidQual.ToString() != ""))
                    acroFields.SetField("app.MidQual", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateMidQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString())));
                if ((dt[0].DiscountRate.ToString() != "") && (dt[0].DiscRateNonQual.ToString() != ""))
                    acroFields.SetField("app.NonQual", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateNonQual) - Convert.ToDecimal(dt[0].DiscountRate.ToString())));
            }
            if (dt[0].DiscRateQualDebit.ToString() != "")
            {
                acroFields.SetField("app.QualFeeOD", dt[0].DiscRateQualDebit.ToString());
                if (dt[0].DiscRateMidQual.ToString() != "")
                {
                    acroFields.SetField("app.MidQualOD", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateMidQual) - Convert.ToDecimal(dt[0].DiscRateQualDebit)));
                    acroFields.SetField("app.NonQualOD", Convert.ToString(Convert.ToDecimal(dt[0].DiscRateNonQual) - Convert.ToDecimal(dt[0].DiscRateQualDebit)));
                }
            }

            acroFields.SetField("app.Gateway", dt[0].Gateway);
            acroFields.SetField("app.GatewayMonthlyAccess", dt[0].GatewayMonFee.ToString());
            acroFields.SetField("app.GatewayTransationFee", dt[0].GatewayTransFee.ToString());
            #endregion

            #region Banking
            //Baking
            acroFields.SetField("app.DiscoverNum", dt[0].PrevDiscoverNum);
            acroFields.SetField("app.AmexNum", dt[0].PrevAmexNum);
            acroFields.SetField("app.JCBNum", dt[0].PrevJCBNum); acroFields.SetField("app.BankName", dt[0].BankName);
            acroFields.SetField("app.BankAddress", dt[0].BankAddress);
            acroFields.SetField("app.BankCity", dt[0].BankCity);
            acroFields.SetField("app.BankState", dt[0].BankState);
            acroFields.SetField("app.BankZip", dt[0].BankZip);
            acroFields.SetField("app.BankPhone", dt[0].BankPhone);
            acroFields.SetField("app.RoutingNum", dt[0].BankRoutingNumber);
            acroFields.SetField("app.AcctNum", dt[0].BankAccountNumber);
            //acroFields.SetField("app.BankContactName", dt[0].NameOnCheckingAcct);
            #endregion

            #region Platform
            if (dt[0].Platform.ToString().Contains("Omaha"))
                acroFields.SetField("app.chkOmaha", "Yes");
            else if (dt[0].Platform.ToString().Contains("Nashville"))
                acroFields.SetField("app.chkNashville", "Yes");
            else if (dt[0].Platform.ToString().Contains("Vital"))
                acroFields.SetField("app.chkVital", "Yes");
            else if (dt[0].Platform.ToString().Contains("North"))
                acroFields.SetField("app.chkNorth", "Yes");
            else if ((dt[0].Platform != "") && (dt[0].Platform.ToLower() != "none"))
            {
                acroFields.SetField("app.chkFrontEndOther", "Yes");
                acroFields.SetField("app.OtherPlatform", dt[0].Platform);
            }
            #endregion
            */
            stamper.FormFlattening = true;
            stamper.Close();
            //Response.OutputStream.Write(mStream.GetBuffer(), 0, mStream.GetBuffer().Length - 1);
            //Response.Flush();
            //Response.Close();

            DisplayMessage("PDF created in the customer folder - " + FilePath);
            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("IPayment Data not found for this record.");
            return false;
        }
    }//end function CreateIPayPDF
    #endregion

    #region MERRICK PDF
    //This function creates Optimal-Merrick PDF
    public bool CreateMerrickPDF(string ContactID)
    {
        //Get data for Merrick Application
        PDFBL MerrickData = new PDFBL();
        PartnerDS.ACTMerrickPDFDataTable dt = MerrickData.GetMerrickDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            //Populate data in PDF
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/Merrick App.pdf"));
            //MemoryStream mStream = new MemoryStream();
            //PdfStamper stamper = new PdfStamper(reader, mStream);
            //stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting | PdfWriter.AllowFillIn | PdfWriter.AllowModifyContents | PdfWriter.AllowAssembly);
            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "../PDF/Merrick App_" + ContactID + ".pdf";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                if (P1LastName == "")
                    P1LastName = "Merchant";
                
                strPath = Server.MapPath(strHost + FilePath + "/" + "Merrick App_" + P1FirstName.Substring(0, 1) + P1LastName + ".pdf");
            }
            //MemoryStream mStream = new MemoryStream();
            //PdfStamper stamper = new PdfStamper(reader, mStream);
            //stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            AcroFields acroFields = stamper.AcroFields;

            #region General Information

            acroFields.SetField("MerchantName", dt[0].DBA);

            //if Company Name different than DBA
            if (dt[0].COMPANYNAME != dt[0].DBA)
                acroFields.SetField("CorporateName", dt[0].COMPANYNAME);
            
            acroFields.SetField("MerchantAddress", dt[0].Address1 + dt[0].Address2);
            acroFields.SetField("MerchantCity", dt[0].CITY);
            acroFields.SetField("MerchantState", dt[0].STATE);
            acroFields.SetField("MerchantZip", dt[0].ZipCode);

            acroFields.SetField("CorporateAddress", dt[0].BillingAddress1 + dt[0].BillingAddress2);
            acroFields.SetField("CorporateCity", dt[0].BillingCity);
            acroFields.SetField("CorporateState", dt[0].BillingState);
            acroFields.SetField("CorporateZip", dt[0].BillingZipCode);
     
            acroFields.SetField("ContactEmail", dt[0].Email);
            acroFields.SetField("ContactName", dt[0].ContactName);
            acroFields.SetField("WebSite", dt[0].Website);
            acroFields.SetField("ContactPhone", dt[0].P1PhoneNumber);
            acroFields.SetField("ContactFax", dt[0].Fax);
            acroFields.SetField("ContactCS", dt[0].P1PhoneNumber);
            acroFields.SetField("TaxID", dt[0].FederalTaxID);
            acroFields.SetField("BusinessName", dt[0].COMPANYNAME);

            acroFields.SetField("AverageTicket", dt[0].AverageTicket.ToString());
            acroFields.SetField("HighestTicket", dt[0].MaxTicket.ToString());
            acroFields.SetField("MonthlyVolume", dt[0].MonthlyVolume.ToString());

            acroFields.SetField("ProdDesc1", dt[0].ProductSold);
            acroFields.SetField("YearsBusiness", dt[0].YIB.ToString());

          
            if (dt[0].LegalStatus == "Corporation")
                acroFields.SetField("Ownership", "1");
            else if (dt[0].LegalStatus == "Sole Proprietorship")
                acroFields.SetField("Ownership", "2");
            else if (dt[0].LegalStatus == "Partnership")
                acroFields.SetField("Ownership", "3");
            else if (dt[0].LegalStatus == "Government")
                acroFields.SetField("Ownership", "4");
            else if (dt[0].LegalStatus == "Non-Profit")
                acroFields.SetField("Ownership", "6");
            else if (dt[0].LegalStatus == "LLC")
                acroFields.SetField("Ownership", "7");
            else 
                acroFields.SetField("StateOwnership", dt[0].LegalStatus);

      
            if (dt[0].PrevProcessed == "Yes")
                acroFields.SetField("PaymentCards", "1");
            else if (dt[0].PrevProcessed == "No")
                acroFields.SetField("PaymentCards", "2");
            acroFields.SetField("ReasonLeaving", dt[0].ReasonForLeaving);

            #endregion

            #region Principal #1
            //Principal #1
            acroFields.SetField("PrincipalFirst1", dt[0].P1FirstName);
            acroFields.SetField("PrincipalLast1", dt[0].P1LastName);
            acroFields.SetField("PrincipalMiddle1", dt[0].P1MName);
            acroFields.SetField("Principal%1", dt[0].P1OwnershipPercent.ToString());
            acroFields.SetField("PrinicpalSSN1", dt[0].P1SSN);
            acroFields.SetField("PrincipalDriver1", dt[0].P1DriversLicenseNo);
            acroFields.SetField("PrincipalTitle1", dt[0].P1Title);
            acroFields.SetField("PrincipalAddress1", dt[0].P1Address);
            acroFields.SetField("PrincipalCity1", dt[0].P1City);
            acroFields.SetField("PrincipalState1", dt[0].P1State);
            acroFields.SetField("PrincipalZip1", dt[0].P1ZipCode);
            acroFields.SetField("PrincipalDOB1", dt[0].P1DOB);
            acroFields.SetField("PrinicpalPhone1", dt[0].P1PhoneNumber);
            //acroFields.SetField("PrinicpalCell1", dt[0].P1MobilePhone);
            acroFields.SetField("PrinicpalEmail1", dt[0].P1PhoneNumber);
     
            #endregion

            #region Principal #2
            //Principal #2
            acroFields.SetField("PrincipalFirst2", dt[0].P2FirstName);
            acroFields.SetField("PrincipalLast2", dt[0].P2LastName);
            //acroFields.SetField("PrincipalMiddle2", dt[0].P2MName);
            acroFields.SetField("Principal%2", dt[0].P2OwnershipPercent.ToString());
            acroFields.SetField("PrinicpalSSN2", dt[0].P2SSN);
            acroFields.SetField("PrincipalDriver2", dt[0].P2DriversLicenseNo);
            acroFields.SetField("PrincipalTitle2", dt[0].P2Title);
            acroFields.SetField("PrincipalAddress2", dt[0].p2Address);
            acroFields.SetField("PrincipalCity2", dt[0].P2City);
            acroFields.SetField("PrincipalState2", dt[0].P2State);
            acroFields.SetField("PrincipalZip2", dt[0].P2ZipCode);
            acroFields.SetField("PrincipalDOB2", dt[0].P2DOB);
            acroFields.SetField("PrinicpalPhone2", dt[0].p2PhoneNumber);
            //acroFields.SetField("PrincipalCell2", dt[0].P2MobilePhone);
            acroFields.SetField("PrinicpalEmail2", dt[0].P2PhoneNumber);

            #endregion
            if (dt[0].CTMF == "Yes")
                acroFields.SetField("Terminated", "1");
            else if (dt[0].CTMF == "No")
                acroFields.SetField("Terminated", "2");

            #region Banking
            //Banking
            if (dt[0].BankRoutingNumber != "")
            {
                acroFields.SetField("AccountType", "1");
                acroFields.SetField("RoutNumber", dt[0].BankRoutingNumber);
                acroFields.SetField("AcctNumber", dt[0].BankAccountNumber);
            }
   
            //acroFields.SetField("BankContactName", dt[0].NameOnCheckingAcct);
            #endregion

            #region CardPCT
            int MailPhone = Convert.ToInt16(dt[0].BusinessPctMail) + Convert.ToInt16(dt[0].BusinessPctPhone);
            acroFields.SetField("Swiped.ToString()%", dt[0].ProcessPctSwiped.ToString().ToString());
            acroFields.SetField("Moto%", MailPhone.ToString());
            acroFields.SetField("Internet.ToString()%", dt[0].BusinessPctInternet.ToString().ToString());
            acroFields.SetField("GatewayName", dt[0].Gateway);
            

            #endregion

            acroFields.SetField("AmexApply", dt[0].NewAmex);
            acroFields.SetField("AcceptAmex", dt[0].PrevAmexNum);

            acroFields.SetField("EquipType1", dt[0].EquipmentType);
            acroFields.SetField("Model1", dt[0].EquipmentModel);

            #region PrincipalSignatures          
            acroFields.SetField("MerchantTitle1", dt[0].P1Title);
            acroFields.SetField("MerchantPrincipal1", dt[0].P1FullName);

            acroFields.SetField("MerchantTitle2", dt[0].P2Title);
            acroFields.SetField("MerchantPrincipal2", dt[0].P2FullName);

            acroFields.SetField("BankPrincipal1", dt[0].P1FullName);

            acroFields.SetField("GuarantorPrincipal1", dt[0].P1FullName);
            acroFields.SetField("GuarantorPrincipal2", dt[0].P2FullName);
            #endregion


            #region Rates
            //Rates

            acroFields.SetField("STAFM", dt[0].CustServFee.ToString());

            //if Internet.ToString() Account
            if (Convert.ToInt32(dt[0].ProcessPctSwiped.ToString()) >= 50)
            {
                acroFields.SetField("MotoQual", dt[0].DiscQP.ToString());
                acroFields.SetField("MotoMidQual", dt[0].DiscMQStep.ToString());
                acroFields.SetField("MotoNonQual", dt[0].DiscNQStep.ToString());
                acroFields.SetField("MotoBundled", "");

                acroFields.SetField("MotoSetupFeeApp", dt[0].AppFee.ToString());
                acroFields.SetField("MotoSetupFeeRB", "");
                acroFields.SetField("MotoSetupFeeAmex", "");

                acroFields.SetField("MotoMonthlyFee", dt[0].CustServFee.ToString());
                //acroFields.SetField("MotoMonthlyFeeReport", dt[0].Internet.ToString()Stmt);
                acroFields.SetField("MotoMonthlyFeeMin", dt[0].MonMin.ToString());
                acroFields.SetField("MotoMonthlyFeeSecure", "");
                acroFields.SetField("MotoMonthlyFeeRB", "");

                acroFields.SetField("MotoTxnFee", dt[0].TransactionFee.ToString());
                acroFields.SetField("MotoTxnFeeMid", dt[0].TransactionFee.ToString());
                acroFields.SetField("MotoTxnFeeNonQual", dt[0].TransactionFee.ToString());
                acroFields.SetField("MotoTxnFeeAmex", dt[0].TransactionFee.ToString());
                acroFields.SetField("MotoTxnFeeTDS", dt[0].TransactionFee.ToString());

                acroFields.SetField("MotoOtherFeeCB", dt[0].ChargebackFee.ToString());
                acroFields.SetField("MotoOtherFeeACH", "");
                acroFields.SetField("MotoOtherFeeFailed", "");
                acroFields.SetField("MotoOtherFeeAVS", dt[0].AVS.ToString());
                acroFields.SetField("MotoOtherFeeGateway", dt[0].GatewayMonFee.ToString());
                
                acroFields.SetField("MotoOtherFeeAnnual", dt[0].AnnualFee.ToString());
                acroFields.SetField("MotoOtherFeeOther", "");

                acroFields.SetField("ReserveAccount%CNP", dt[0].RollingReserve.ToString());
            }
            else
            {
                acroFields.SetField("RetailQual", dt[0].DiscQP.ToString());
                acroFields.SetField("RetailMidQual", dt[0].DiscMQStep.ToString());
                acroFields.SetField("RetailNonQual", dt[0].DiscNQStep.ToString());
                //acroFields.SetField("RetailOffline", dt[0].DiscQDStep);
                acroFields.SetField("RetailBundled","" );

                acroFields.SetField("RetailSetupFeeApp", dt[0].AppFee.ToString());
                acroFields.SetField("RetailSetupFeeMobile", "");
                acroFields.SetField("RetailSetupFeeAmex", "");

                acroFields.SetField("RetailMonthlyFee", dt[0].CustServFee.ToString());
                //acroFields.SetField("RetailMonthlyFeeReport", dt[0].InternetStmt.ToString());
                acroFields.SetField("RetailMonthlyFeeMin", dt[0].MonMin.ToString());
                acroFields.SetField("RetailMonthlyFeeStatement", "");
                acroFields.SetField("RetailMonthlyFeeMobile", "");  
                acroFields.SetField("RetailMonthlyFeeClub", "");
                acroFields.SetField("RetailMonthlyFeeMonthly", "");  
                acroFields.SetField("RetailMonthlyDiscountMonthly", "");
                  
                acroFields.SetField("RetailTxnFee", dt[0].TransactionFee.ToString());
                acroFields.SetField("RetailTxnFeeMid", dt[0].TransactionFee.ToString());
                acroFields.SetField("RetailTxnFeeNonQual", dt[0].TransactionFee.ToString());
                acroFields.SetField("RetailTxnFeeAmex", dt[0].TransactionFee.ToString());
                acroFields.SetField("RetailTxnFeeDebit", dt[0].DebitTransFee.ToString());
                acroFields.SetField("RetailTxnFeeEBT", dt[0].EBTTransFee.ToString());

                acroFields.SetField("RetailOtherFeeCB", dt[0].ChargebackFee.ToString());
                acroFields.SetField("RetailOtherFeeAuth", "");     
                acroFields.SetField("RetailOtherFeeVoice", dt[0].VoiceAuth.ToString());                           
                acroFields.SetField("RetailOtherFeeAVS", dt[0].AVS.ToString());
                acroFields.SetField("RetailOtherFeeMobile", "");
                acroFields.SetField("RetailOtherFeeBatch", dt[0].BatchHeader.ToString());
                acroFields.SetField("RetailOtherFeeAnnual", dt[0].AnnualFee.ToString());
                acroFields.SetField("RetailOtherFeeWarranty", "");
                
                acroFields.SetField("RetailOtherFeeOther", "");
                acroFields.SetField("ReserveAccount%", dt[0].RollingReserve.ToString());                        
            }
            #endregion

            
            //**********************INTERNET OR MOTO ACCOUNT Questionaire************************
            if (Convert.ToInt32(dt[0].ProcessPctSwiped.ToString()) < 50)
            {
                //Only populate if its a CNP account
                if (dt[0].RefundPolicy.Contains("Refund within 30"))
                    acroFields.SetField("Refund", "2");
                else if (dt[0].RefundPolicy == "No Refund")
                    acroFields.SetField("Refund", "3");
                else if (dt[0].RefundPolicy != "")
                    acroFields.SetField("Refund", dt[0].OtherRefund);

                acroFields.SetField("ProdDescription1", dt[0].ProductSold);
                acroFields.SetField("Turnaround", dt[0].NumDaysDel.ToString());


                //**********************END IF INTERNET OR MOTO ACCOUNT************************

                //Platform Check boxes            
                stamper.FormFlattening = true;
                stamper.Close();
                //Response.OutputStream.Write(mStream.GetBuffer(), 0, mStream.GetBuffer().Length - 1);
                //Response.Flush();
                //Response.Close();

                DisplayMessage("PDF created in the customer folder - " + FilePath);
            }
            return true;            
        }//end if count > 0
        else
        {
            DisplayMessage("Optimal Merrick Data not found for this record.");
            return false;
        }
    }//end function CreateMerrickPDF
    #endregion

    #region CHASE PDF
    //This function creates Chase About Merchant PDF
    public bool CreateChasePDFAbout(string ContactID)
    {
        //Get data for Chase PDF
        PDFBL ChaseData = new PDFBL();
        PartnerDS.ACTChasePDFDataTable dt = ChaseData.GetChaseDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            //Populate data in PDF
            //Put the chase PDF in the PDF folder in Partner and name the PDF accordingly
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/Chase About Merchant.pdf"));
            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "../PDF/Chase About_" + ContactID + ".pdf";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                if (P1LastName == "")
                    P1LastName = "Merchant";

                strPath = Server.MapPath(strHost + FilePath + "/" + "Chase About_" + P1FirstName + " " + P1FirstName + ".pdf");
            }
            //MemoryStream mStream = new MemoryStream();
            //PdfStamper stamper = new PdfStamper(reader, mStream);
            //stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            AcroFields acroFields = stamper.AcroFields;

            #region Banking
            //Banking
            acroFields.SetField("Banking Info:First/Last Contact Name", "Manager");
            acroFields.SetField("Banking Info:Phone Number", dt[0].BankPhone);

            #endregion

            #region General Information
            acroFields.SetField("About Merchant's Bus.:DBA Name", dt[0].DBA);

            acroFields.SetField("Checklist Info:MCC", dt[0].MCC);
            acroFields.SetField("Checklist Info: Rep #", dt[0].RepNum);
            acroFields.SetField("Checklist Info: Print Sales Rep. Name", dt[0].RepName);

            //Refund Policy from View is 1,2, or 3
            acroFields.SetField("Client Visitation: 10. Return Policy", dt[0].RefundPolicy.ToString());

            //Visa Master Refund Policy is boolean 0 or 1
            acroFields.SetField("Client Visitation: 11. Do you have refund for MC/VISA", dt[0].VisaMasterRefund.ToString());


            if (dt[0].ThreeMonthsPrev.ToString() == "1")
                acroFields.SetField("Client Visitation: 11. Do you have Prev Processor MC/VISA Statements", "Yes");
            else
                acroFields.SetField("Client Visitation: 11. Do you have Prev Processor MC/VISA Statements", "No");

            if (dt[0].BillingSame.ToString() == "1")
            {
                acroFields.SetField("Mail Statements/Documents:  Bill To Name", dt[0].COMPANYNAME);
                acroFields.SetField("Mail Statements/Documents:  Contact Name", dt[0].P1FullName);
                acroFields.SetField("Mail Statements/Documents:  Address", dt[0].BillingAddress);
                acroFields.SetField("Mail Statements/Documents:  City", dt[0].BillingCity);
                acroFields.SetField("Mail Statements/Documents:  State", dt[0].BillingState);
                acroFields.SetField("Mail Statements/Documents:: Zip", dt[0].BillingZipCode);
            }
            acroFields.SetField("Client Visitation: 13. Previous Processor", dt[0].PrevProcessor);
            acroFields.SetField("Client Visitation: 14. Previous Merchant Number", dt[0].PrevMerchantNum);

            acroFields.SetField("Client Visitation: 17. Email", dt[0].Email);

            acroFields.SetField("Client Visitation: 19.  Are customers required to have a deposit, time frame", dt[0].NumOfDaysProdDel);

            acroFields.SetField("Processing Information: 7.Debit Cash Back", dt[0].OnlineDebit.ToString());
            #endregion
            stamper.FormFlattening = true;
            stamper.Close();
            //Response.OutputStream.Write(mStream.GetBuffer(), 0, mStream.GetBuffer().Length - 1);
            //Response.Flush();
            //Response.Close();

            DisplayMessage("Chase About PDF created in the customer folder - " + FilePath);
            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("Chase Data not found for this record.");
            return false;
        }
    }//end function CreateChaseAboutPDF

    //This function creates Chase PDF Fee Schedule
    public bool CreateChasePDFFS(string ContactID)
    {
        //Get data for Chase PDF
        PDFBL PDF = new PDFBL();
        PartnerDS.ACTChasePDFDataTable dt = PDF.GetChaseDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            //Populate data in PDF
            //Put the chase PDF in the PDF folder in Partner and name the PDF accordingly
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/Chase Fee Schedule.pdf"));
            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "../PDF/Chase FS_" + ContactID + ".pdf";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1LastName == "")
                    P1LastName = "Merchant";
                strPath = Server.MapPath(strHost + FilePath + "/" + "Chase Fee Schedule_" + P1FirstName + " " + P1LastName + " " + ".pdf");
            }
            //MemoryStream mStream = new MemoryStream();
            //PdfStamper stamper = new PdfStamper(reader, mStream);
            //stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            AcroFields acroFields = stamper.AcroFields;

            acroFields.SetField("Service Fee Scgedule: DBA Name", dt[0].DBA);
            acroFields.SetField("Service Fee Schedule: Loc", "1");
            acroFields.SetField("Service Fee Schedule: Loc of", dt[0].NumOfLocs);


            #region Rates
            //Rates
            acroFields.SetField("Service Fee Schedule: Discount Fees: MC Quailified Credit Discount Rate", dt[0].DiscountRate.ToString());
            acroFields.SetField("Service Fee Schedule: Discount Fees: Visa Quailified Credit Discount Rate", dt[0].DiscountRate.ToString());
            acroFields.SetField("Service Fee Schedule: Discount Fees: MC Quailified Debit Discount Rate", dt[0].DiscountRateDebit.ToString());
            acroFields.SetField("Service Fee Schedule: Discount Fees: Visa Quailified Debit Discount Rate", dt[0].DiscountRateDebit.ToString());
            acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: Service Fee", dt[0].CustServFee.ToString());
            acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: Chargeback Fee", dt[0].ChargebackFee.ToString());
            //acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: Chargeback Retrieval Fee", dt[0].RetrievalFee.ToString());
            acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: ACH Fee Per Transfer", dt[0].BatchHeader.ToString());
            acroFields.SetField("Service Fee Schedule: Other Fees: Authorization: MC (Unbundled)", dt[0].TransactionFee.ToString());
            acroFields.SetField("Service Fee Schedule: Other Fees: Authorization: Visa Unbundled", dt[0].TransactionFee.ToString());

            //set Amex only if not Opted Out
            //if (dt[0].AmexAccept == "1")  
            acroFields.SetField("Service Fee Schedule: Other Fees: Authorization: American Express", dt[0].NBCTransFee.ToString());


            //set Amex only if not Opted Out
            //if (dt[0].DiscoverAccept == "1")
            acroFields.SetField("Service Fee Schedule: Other Fees: Authorization: Discover", dt[0].NBCTransFee.ToString());

            acroFields.SetField("Service Fee Schedule: Other Fees: Authorization: JCB", dt[0].NBCTransFee.ToString());
            //if (dt[0].JCBAccept == "1")
            acroFields.SetField("Service Fee Schedule: Other Fees: Authorization: Diners Club", dt[0].NBCTransFee.ToString());

            acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: Minimum Processing Fee", dt[0].MonMin.ToString());
            acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: Annual Membership Fee", dt[0].AnnualFee.ToString());
            acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: Internet.ToString() Paper Statement Fee", dt[0].IPSFee.ToString());
            acroFields.SetField("Service Fee Schedule: Billed Monthly Fees: Wireless Access Fee", dt[0].WirelessAccess.ToString());
            acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: MC/Visa VRU/Voice", dt[0].VoiceAuth.ToString());
            acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: AVS-Auto", dt[0].AVS.ToString());
            //acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: AVS-Manual", dt[0].VoiceAuth.ToString());
            acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: EBT", dt[0].EBTTransFee.ToString());
            acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: Debit (PIN)", dt[0].DebitTransFee.ToString());

            if (Convert.ToBoolean(dt[0].Interchange))
            {
                if (Convert.ToBoolean(dt[0].Assessments))
                {
                    acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: Accessment Fees", ".095,.0925");//Mastercard and Visa                    
                }
                acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: Other", "0");
                acroFields.SetField("Service Fee Schedule: Other Fees: MC/VI Foreign: Other Code", "550, 560");
            }

            #endregion
            acroFields.SetField("Service Fee Schedule: Printed Name of Signer", dt[0].P1FullName);
            acroFields.SetField("Service Fee Schedule: Title", dt[0].P1Title);
            stamper.FormFlattening = true;


            stamper.Close();
            //Response.OutputStream.Write(mStream.GetBuffer(), 0, mStream.GetBuffer().Length - 1);
            //Response.Flush();
            //Response.Close();

            DisplayMessage("Chase Fee Schedule PDF created in the customer folder - " + FilePath);
            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("Chase Data not found for this record.");
            return false;
        }
    }//end function CreateChaseAboutPDF

    //This function creates Chase PDF
    public bool CreateChasePDFMP(string ContactID)
    {
        //Get data for Chase PDF
        PDFBL PDF = new PDFBL();
        PartnerDS.ACTChasePDFDataTable dt = PDF.GetChaseDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            //Populate data in PDF
            //Put the chase PDF in the PDF folder in Partner and name the PDF accordingly
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/Chase MPA.pdf"));
            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "../PDF/Chase MPA_" + ContactID + ".pdf";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";
                strPath = Server.MapPath(strHost + FilePath + "/" + "Chase MPA_" + P1FirstName + ".pdf");
            }
            //MemoryStream mStream = new MemoryStream();
            //PdfStamper stamper = new PdfStamper(reader, mStream);
            //stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            AcroFields acroFields = stamper.AcroFields;

            acroFields.SetField("Tell Us About Your Business: Client", dt[0].COMPANYNAME);
            acroFields.SetField("Service Fee Scgedule: DBA Name", dt[0].DBA);
            acroFields.SetField("Merchant Processing Application: Of", dt[0].NumOfLocs);
            #region General Information

            if (dt[0].COMPANYNAME == dt[0].DBA)
                acroFields.SetField("Tell Us About Your Business: Same as Legal Name", "1");

            else
                acroFields.SetField("Tell Us About Your Business: DBA/Outlet Name", dt[0].DBA);

            acroFields.SetField("Tell Us About Your Business: Contact Name", dt[0].P1FullName);
            acroFields.SetField("Tell Us About Your Business: Address", dt[0].Address1);
            acroFields.SetField("Tell Us About Your Business: Suite", dt[0].Address2);

            acroFields.SetField("Tell Us About Your Business: City", dt[0].CITY);
            acroFields.SetField("Tell Us About Your Business: State", dt[0].STATE);
            acroFields.SetField("Tell Us About Your Business: Zipcode", dt[0].ZipCode);
            acroFields.SetField("Tell Us About Your Business: Fax Phone", dt[0].Fax);
            acroFields.SetField("Tell Us About Your Business: Business Phone", dt[0].BusinessPhone);

            if (dt[0].FederalTaxID != "")
            {
                acroFields.SetField("Provide More Business Data: Fed. Tax ID", dt[0].FederalTaxID);
                acroFields.SetField("Provide More Business Data: TIN Type", "1");
            }
            else if (dt[0].P1SSN != "")
            {
                acroFields.SetField("Provide More Business Data: Fed. Tax ID", dt[0].P1SSN);
                acroFields.SetField("Provide More Business Data: TIN Type", "2");
            }
            acroFields.SetField("Describe Equipment Details: VAR/Internet.ToString()/Software", dt[0].Gateway);

            acroFields.SetField("Tell Us About Your Business: Same as Business Phone", dt[0].SameAsBusinessPhone.ToString());
            if (dt[0].BusinessPhone != dt[0].CustServPhone)
                acroFields.SetField("Tell Us About Your Business: Merchant's Customer Service Phone", dt[0].CustServPhone);

            //Business Type is 1 to 7
            acroFields.SetField("Provide More Business Data: Business Type", dt[0].LegalStatus.ToString());

            acroFields.SetField("Tell Us About Your Business:  Average Ticket/Sales Amount", dt[0].AvgTicket.ToString());
            acroFields.SetField("Tell Us About Your Business:  Annual MC/Visa Volume", dt[0].AnnualVol.ToString());
            acroFields.SetField("Provide More Business Data: Month/Yr Started", dt[0].StartYear.ToString());
            acroFields.SetField("Provide More Business Data: Products/Services You Sell", dt[0].ProductSold);
            acroFields.SetField("Other Entitlements: Non-Lic. JCB (existing account)", dt[0].JCBNum);
            acroFields.SetField("Other Entitlemen:Discover (EDC)", dt[0].DiscoverNum);
            acroFields.SetField("Other Entitlements:Amer. Exp", dt[0].AmexNum);
            acroFields.SetField("Other Entitlements", dt[0].JCBAccept);
            acroFields.SetField("Other Entitlements", dt[0].DiscoverAccept);
            acroFields.SetField("Other Entitlements:", dt[0].AmexAccept);

            acroFields.SetField("Describe Equipment Details: VAR/Internet.ToString()/Software", dt[0].Gateway);
            #endregion

            #region CardPCT
            acroFields.SetField("Provide More Business Data: Mag Swipe", dt[0].ProcessPctSwiped.ToString());
            acroFields.SetField("Provide More Business Data: Keyed.ToString() Manually", dt[0].ProcessPctKeyed.ToString());
            acroFields.SetField("Provide More Business Data: POS Cardswipe", dt[0].BusinessPctPOS.ToString());
            acroFields.SetField("Provide More Business Data: Mail Order.ToString()", dt[0].BusinessPctMailOrder.ToString());
            acroFields.SetField("Provide More Business Data: Phone Order.ToString()", dt[0].BusinessPctPhoneOrder.ToString());
            acroFields.SetField("Provide More Business Data: Tradeshows", dt[0].BusinessPctTradeShows.ToString());
            acroFields.SetField("Provide More Business Data: Internet.ToString()", dt[0].BusinessPctInternet.ToString());
            #endregion

            #region Principal #1
            //Principal #1
            acroFields.SetField("Provide Your Owner Info:ZIP", dt[0].P1ZipCode);
            acroFields.SetField("Provide Your Owner Info:State", dt[0].P1State);
            acroFields.SetField("Provide Your Owner Info:City", dt[0].P1City);
            acroFields.SetField("Provide Your Owner Info:Home Address", dt[0].P1Address);
            acroFields.SetField("Provide Your Owner Information:Title", dt[0].p1TitleID.ToString());
            if (dt[0].p1TitleID.ToString() == "6")
                acroFields.SetField("Provide Your Owner Information:Text", dt[0].P1Title);
            acroFields.SetField("Provide Your owner Info:Social Security", dt[0].P1SSN);
            acroFields.SetField("Provide Your owner Info:Owner/Partner", dt[0].P1FullName);
            //P1 Ownership field is correct, though labeled as 2 in the PDF
            acroFields.SetField("Provide Your owner Info:2.  % of owner", dt[0].P1OwnershipPercent.ToString());
            acroFields.SetField("Provide Your owner Info:Phone Number", dt[0].P1PhoneNumber);

            acroFields.SetField("ProvideYour Owner Information: Print Name", dt[0].P1FullName);
            acroFields.SetField("ProvideYour Owner Info: Title", dt[0].P1Title);

            #endregion

            #region Principal #2
            //Principal #2
            acroFields.SetField("Provide Your Owner Info: 2. ZIP", dt[0].P2ZipCode);
            acroFields.SetField("Provide Your Owner Info:2. State", dt[0].P2State);
            acroFields.SetField("Provide Your Owner Info:2. City", dt[0].P2City);
            acroFields.SetField("Provide Your Owner Info: 2. Home Address", dt[0].p2Address);
            acroFields.SetField("Provide Your Owner Info: 2 Title", dt[0].P2TitleID.ToString());
            acroFields.SetField("Provide Your Owner Information: 2Text", dt[0].P2Title);
            acroFields.SetField("Provide Your owner Info:2. Social Security", dt[0].P2SSN);
            acroFields.SetField("Provide Your owner Info: 2 . Owner/Partner", dt[0].p2FullName);
            //P2 Ownership Pct is correct, though labeled as P1 in PDF
            acroFields.SetField("Provide Your owner Info:% of owner", dt[0].P2OwnershipPercent.ToString());
            acroFields.SetField("Provide Your owner Info: 2. Phone Number", dt[0].p2PhoneNumber);
            
            #endregion

            if (dt[0].Platform.ToLower().Contains("nashville"))
                acroFields.SetField("Other Entitlements:Network", "2");
            else if (dt[0].Platform.ToLower().Contains("north"))
                acroFields.SetField("Other Entitlements:Network", "1");
            stamper.FormFlattening = true;
            stamper.Close();
            //Response.OutputStream.Write(mStream.GetBuffer(), 0, mStream.GetBuffer().Length - 1);
            //Response.Flush();
            //Response.Close();

            DisplayMessage("Chase MPS PDF created in the customer folder - " + FilePath);
            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("Chase Data not found for this record.");
            return false;
        }
    }//end function CreateChasePDFMP

    //This function creates Chase PDF for Multiple Locations
    public bool CreateChasePDFMPMultipleLocation(string ContactID, bool b3Locs)
    {
        //Get data for Chase PDF
        PDFBL ChaseData = new PDFBL();
        PartnerDS.ACTChasePDFDataTable dt = ChaseData.GetChaseDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            //Populate data in PDF
            //Put the chase PDF in the PDF folder in Partner and name the PDF accordingly
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/Chase Multiple Locations.pdf"));
            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "../PDF/Chase MPA_Multiple Locations_" + ContactID + ".pdf";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1LastName == "")
                    P1LastName = "Merchant";

                strPath = Server.MapPath(strHost + FilePath + "/" + "Chase MPA_Multiple Locations_" + P1FirstName + " " + P1LastName + ".pdf");
            }
            FileStream fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);
            AcroFields acroFields = stamper.AcroFields;

            acroFields.SetField("Client: Business Legal Name", dt[0].COMPANYNAME);
            acroFields.SetField("Location_ of", dt[0].NumOfLocs);
            acroFields.SetField("Location", "2");
            #region General Information

            if (dt[0].COMPANYNAME == dt[0].DBA)
                acroFields.SetField("Same as Legal Name", "Yes");
            else
                acroFields.SetField("Your DBA/Outlet Name:", dt[0].DBA);

            acroFields.SetField("First Name Contact", dt[0].P1FullName);

            if (dt[0].FederalTaxID != "")
            {
                acroFields.SetField("Fed Tax ID", dt[0].FederalTaxID);
                acroFields.SetField("TIN Type", "1");
            }
            else if (dt[0].P1SSN != "")
            {
                acroFields.SetField("Fed Tax ID", dt[0].P1SSN);
                //acroFields.SetField("TIN Type", "2");
            }
            acroFields.SetField("EQUIPMENT: VAR/ Internet.ToString()/ Software: Name", dt[0].Gateway);

            //acroFields.SetField("Tell Us About Your Business: Same as Business Phone", dt[0].SameAsBusinessPhone);
            if (dt[0].BusinessPhone != dt[0].CustServPhone)
                acroFields.SetField("Merchant's Customer Service Phone", dt[0].CustServPhone);

            acroFields.SetField("Products ans Services", dt[0].ProductSold);
            acroFields.SetField("Other Entitlements: Non-Lic. JCB #", dt[0].JCBNum);
            acroFields.SetField("Other Entitlements: Discover #", dt[0].DiscoverNum);
            acroFields.SetField("Other Entitlements: Amer. Exp #", dt[0].AmexNum);
            if (dt[0].JCBAccept == "Yes")
                acroFields.SetField("Other Entitlements: JCB", "1");
            if (dt[0].DiscoverAccept == "Yes")
                acroFields.SetField("Other Entitlements:  Discover", "1");
            if (dt[0].AmexAccept == "Yes")
                acroFields.SetField("Other Entitlements:  Amer. Exp", "1");

            acroFields.SetField("Describe Equipment Details: VAR/Internet.ToString()/Software", dt[0].Gateway);
            #endregion

            #region CardPCT
            acroFields.SetField("Mag Swipe", dt[0].ProcessPctSwiped.ToString());
            acroFields.SetField("Keyed.ToString() Manually", dt[0].ProcessPctKeyed.ToString());
            acroFields.SetField("POS Carswipe/Manual Imprint", dt[0].BusinessPctPOS.ToString());
            acroFields.SetField("Mail Order.ToString()", dt[0].BusinessPctMailOrder.ToString());
            acroFields.SetField("Phone Order.ToString()", dt[0].BusinessPctPhoneOrder.ToString());
            acroFields.SetField("Internet.ToString()", dt[0].BusinessPctInternet.ToString());
            acroFields.SetField("Tradeshows", dt[0].BusinessPctTradeShows.ToString());
            #endregion

            if (dt[0].Platform.ToLower().Contains("nashville"))
                acroFields.SetField("Other Entitlements:  Network", "2");
            else if (dt[0].Platform.ToLower().Contains("north"))
                acroFields.SetField("Other Entitlements:  Network", "1");

            if (b3Locs)
            {
                //Populate the second half of the PDF for location 3
                acroFields.SetField("2Client: Business Legal Name", dt[0].COMPANYNAME);
                acroFields.SetField("2Location_ of", dt[0].NumOfLocs);
                acroFields.SetField("2Location", "3");
                #region General Information

                if (dt[0].COMPANYNAME == dt[0].DBA)
                    acroFields.SetField("2Same as Legal Name", "Yes");
                else
                    acroFields.SetField("2Your DBA/Outlet Name:", dt[0].DBA);

                acroFields.SetField("2First Name Contact", dt[0].P1FullName);

                if (dt[0].FederalTaxID != "")
                {
                    acroFields.SetField("2Fed Tax ID", dt[0].FederalTaxID);
                    acroFields.SetField("2TIN Type", "1");
                }
                else if (dt[0].P1SSN != "")
                {
                    acroFields.SetField("2Fed Tax ID", dt[0].P1SSN);
                    acroFields.SetField("2TIN Type", "2");
                }
                acroFields.SetField("2EQUIPMENT: VAR/ Internet.ToString()/ Software: Name", dt[0].Gateway);

                //acroFields.SetField("Tell Us About Your Business: Same as Business Phone", dt[0].SameAsBusinessPhone);
                if (dt[0].BusinessPhone != dt[0].CustServPhone)
                    acroFields.SetField("2Merchant's Customer Service Phone", dt[0].CustServPhone);

                acroFields.SetField("2Products ans Services", dt[0].ProductSold);
                acroFields.SetField("2Other Entitlements: Non-Lic. JCB #", dt[0].JCBNum);
                acroFields.SetField("2Other Entitlements: Discover #", dt[0].DiscoverNum);
                acroFields.SetField("2Other Entitlements: Amer. Exp #", dt[0].AmexNum);
                if (dt[0].JCBAccept == "Yes")
                    acroFields.SetField("2Other Entitlements: JCB", "1");
                if (dt[0].DiscoverAccept == "Yes")
                    acroFields.SetField("2Other Entitlements:  Discover", "1");
                if (dt[0].AmexAccept == "Yes")
                    acroFields.SetField("2Other Entitlements:  Amer. Exp", "1");

                acroFields.SetField("2Describe Equipment Details: VAR/Internet.ToString()/Software", dt[0].Gateway);
                #endregion

                #region CardPCT
                acroFields.SetField("2Mag Swipe", dt[0].ProcessPctSwiped.ToString().ToString());
                acroFields.SetField("2Keyed.ToString() Manually", dt[0].ProcessPctKeyed.ToString().ToString());
                acroFields.SetField("2POS Carswipe/Manual Imprint", dt[0].BusinessPctPOS.ToString());
                acroFields.SetField("2Mail Order.ToString()", dt[0].BusinessPctMailOrder.ToString().ToString());
                acroFields.SetField("2Phone Order.ToString()", dt[0].BusinessPctPhoneOrder.ToString().ToString());
                acroFields.SetField("2Internet.ToString()", dt[0].BusinessPctInternet.ToString().ToString());
                acroFields.SetField("2Tradeshows", dt[0].BusinessPctTradeShows.ToString().ToString());
                #endregion

                if (dt[0].Platform.ToLower().Contains("nashville"))
                    acroFields.SetField("2Other Entitlements:  Network", "2");
                else if (dt[0].Platform.ToLower().Contains("north"))
                    acroFields.SetField("2Other Entitlements:  Network", "1");
            }//end if 3 locations 

            stamper.FormFlattening = true;
            stamper.Close();           
            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("Chase Data not found for this record.");
            return false;
        }
    }//end function CreateChasePDFMPMultipleLocation

    //This function creates Chase PDF Addendum
    public bool CreateChasePDFCreditAdd(string ContactID)
    {
        //Get data for Chase PDF
        PDFBL PDF= new PDFBL();
        PartnerDS.ACTChasePDFDataTable dt = PDF.GetChaseDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            //Populate data in PDF
            //Put the chase PDF in the PDF folder in Partner and name the PDF accordingly
            PdfReader reader = new PdfReader(Server.MapPath("../PDF/Chase Credit Addendum.pdf"));
            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "../PDF/Chase Credit Addendum_" + ContactID + ".pdf";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1LastName == "")
                    P1LastName = "Merchant";

                strPath = Server.MapPath(strHost + FilePath + "/" + "Chase Addendum_" + P1FirstName + " " +  P1LastName + ".pdf");
            }

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            AcroFields acroFields = stamper.AcroFields;
            acroFields.SetField("Merchant Processing Credit Addendum", dt[0].DBA);
            acroFields.SetField("Other Enclosures 4. Internet.ToString() (required) list Web site address", dt[0].Website);
            if (dt[0].Website != "")
                acroFields.SetField("Other Enclosures 4. List Web Site Address", "Yes");

            //if ( ( Convert.ToInt32(dt[0].NumofDaysProdDel.ToString()  >= 0 ) && (Convert.ToInt32(dt[0].NumofDaysProdDel )  <= 7 )  )
            //  acroFields.SetField("Mail/Telephone Order.ToString() 3. 0-7 days", "100");

            stamper.FormFlattening = true;


            stamper.Close();

            DisplayMessage("Chase Addendum PDF created in the cusomter folder - " + FilePath);
            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("Chase Data not found for this record.");
            return false;
        }
    }//end function CreateChasePDFCreditAdd

    protected void btnChaseAbout_Click(object sender, EventArgs e)
    {
        try
        {
            CreateChasePDFAbout(selContactID);
        }//end try
        catch (Exception err)
        {
            DisplayMessage(err.Message);
        }
    }

    protected void btnChaseFee_Click(object sender, EventArgs e)
    {
        try
        {
            CreateChasePDFFS(selContactID);
        }//end try
        catch (Exception err)
        {
            DisplayMessage(err.Message);
        }
    }

    protected void btnChaseMP_Click(object sender, EventArgs e)
    {
        try
        {
            //Get data for Chase PDF
            PDFBL ChaseData = new PDFBL();
            PartnerDS.ACTChasePDFDataTable dt = ChaseData.GetChaseDataFromACT(selContactID);
            if (dt.Rows.Count > 0)
            {
                if (dt[0].NumOfLocs.Trim() != "")
                {
                    if (Convert.ToInt32(dt[0].NumOfLocs) >= 1)
                    {
                        CreateChasePDFMP(selContactID);//Create MPA PDF for first location
                        if (Convert.ToInt32(dt[0].NumOfLocs) == 2)
                        {
                            CreateChasePDFMPMultipleLocation(selContactID, false);//Create Multiple location PDF for location 2                            
                        }//end if num locs is 2
                        else if (Convert.ToInt32(dt[0].NumOfLocs) > 2)
                        {
                            CreateChasePDFMPMultipleLocation(selContactID, true);//Create Multiple location PDF for loc 3                            
                        }//end if number of locs is > 2
                    }//end if numlocs is >= 1
                }//end if numlocs is not blank
                else // If Num Locs not entered then default it to 1 of 1 and create the chase mpa PDF
                {
                    CreateChasePDFMP(selContactID);
                }
            }//end if count not 0            
        }//end try
        catch (Exception err)
        {
            DisplayMessage(err.Message);
        }
    }

    protected void btnChaseCreditAdd_Click(object sender, EventArgs e)
    {
        try
        {
            CreateChasePDFCreditAdd(selContactID);
        }//end try
        catch (Exception err)
        {
            DisplayMessage(err.Message);
        }
    }
    #endregion
    //This function displays error message on a label
    protected void DisplayMessage(string errText)
    {
        lblError.Visible = true;
        lblError.Text = errText;
    }//end function set error message


    #region INTERNATIONAL PDF
    public int CreateInternationalPDF(string ContactID)
    {
        //This function creates International Cal App PDF                
        PDFBL PDF = new PDFBL();
        PartnerDS.ACTOptimalIntlPDFDataTable dt = PDF.GetInternationalDataFromACT(ContactID);
        if (dt.Rows.Count > 0)
        {
            PdfReader reader = new PdfReader(Server.MapPath("/PartnerPortal/PDF/CAL_Application_NA_forms.pdf"));

            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "../PDF/CAL_Application_NA_forms_" + ContactID + ".pdf";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";
                strPath = Server.MapPath(strHost + FilePath + "/" + "CAL_Application_NA_forms_" + P1FirstName.Substring(0, 1) + dt[0].P1LastName + ".pdf");
            }

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);

            AcroFields acroFields = stamper.AcroFields;

            #region General Information
            acroFields.SetField("LegalBusinessName", dt[0].COMPANYNAME);
            acroFields.SetField("DBA", dt[0].DBA);
            acroFields.SetField("WHADWYLT", dt[0].DBA);
            if (dt[0].COMPANYNAME == dt[0].DBA)
                acroFields.SetField("CheckBoxDBA", "Yes");
            acroFields.SetField("Email Address", dt[0].DBA);
            acroFields.SetField("Contact Business Name", dt[0].FIRSTNAME + " " + dt[0].LASTNAME);
            acroFields.SetField("Web Address", dt[0].Website);
            acroFields.SetField("Business Address", dt[0].Address1 + ", " + dt[0].Address2);
            acroFields.SetField("City", dt[0].CITY);

            //No Region field in ACT, so use State
            string RegionCountry = dt[0].STATE.ToString() + "/" + dt[0].Country;

            acroFields.SetField("State", RegionCountry);

            acroFields.SetField("Zip", dt[0].ZipCode);
            if (dt[0].Address1 != dt[0].BillingAddress1)
            {
                //No Region field in ACT, so use State
                string BiRegionCountry = dt[0].BillingState.ToString() + "/" + dt[0].billingCountry;         
         
                acroFields.SetField("Corporate Address", dt[0].BillingAddress1 + ", " + dt[0].BillingAddress2);
                acroFields.SetField("City2", dt[0].BillingCity);
                acroFields.SetField("State2", BiRegionCountry);
                acroFields.SetField("ZiP2", dt[0].BillingZipCode);
            }

            acroFields.SetField("Yrs. in Bus", dt[0].YIB.ToString());
            acroFields.SetField("Bus. Phone", dt[0].BusinessPhonePrefix);
            acroFields.SetField("Bus. Phone 2", dt[0].BusinessPhonePostfix);
            acroFields.SetField("CS  Phone", dt[0].CustServPhonePrefix);
            acroFields.SetField("CS Phone 2", dt[0].CustServPhonePostfix);
            acroFields.SetField("Fax Phone", dt[0].FaxPhonePrefix);
            acroFields.SetField("Fax Phone 2", dt[0].FaxPhonePostfix);
           
            acroFields.SetField("Tax ID", dt[0].FederalTaxID);
            acroFields.SetField("Prod. Sold", dt[0].ProductSold);
            acroFields.SetField("Previous Processor", dt[0].PrevProcessor);

            
            if (dt[0].CTMF == "Yes")
                acroFields.SetField("Button Terminated", "1");
            else
                acroFields.SetField("Button Terminated", "2");

            if (dt[0].LegalStatus == "Sole Proprietorship")
                acroFields.SetField("Company Type", "1");
            if (dt[0].LegalStatus == "Corporation")
                acroFields.SetField("Company Type", "4");
            if (dt[0].LegalStatus == "Partnership")
                acroFields.SetField("Company Type", "2");
            if (dt[0].LegalStatus == "Non-Profit")
                acroFields.SetField("Company Type", "7");
            //if (dt[0].LegalStatus == "Legal/Medical Corp.")
            //  acroFields.SetField("Company Type", "Yes");
            if (dt[0].LegalStatus == "Government")
                acroFields.SetField("Company Type", "6");
            //if (dt[0].LegalStatus == "Tax Exempt")
            //  acroFields.SetField("Company Type", "Yes");
            //if (dt[0].LegalStatus == "Others")
            //  acroFields.SetField("Company Type", "Yes");
            if (dt[0].LegalStatus == "LLC")
                acroFields.SetField("Company Type", "5");
            #endregion

            #region CardPCT
            acroFields.SetField("Mail Order.ToString()", dt[0].BusinessPctMail.ToString());
           acroFields.SetField("Phone Order.ToString()", dt[0].BusinessPctPhone.ToString());
            acroFields.SetField("Trade Show", dt[0].BusinessPctTradeShows.ToString());
            acroFields.SetField("Internet.ToString()", dt[0].BusinessPctInternet.ToString());
      
            #endregion

            #region Principal #1
            //Principal #1
            acroFields.SetField("Owner Zip", dt[0].P1ZipCode);
            acroFields.SetField("Owner State", dt[0].P1State);
            acroFields.SetField("Owner City", dt[0].P1City);
            acroFields.SetField("Home Address", dt[0].P1Address);
            acroFields.SetField("Title", dt[0].P1Title);
            acroFields.SetField("SSN", dt[0].P1SSN);
            acroFields.SetField("Owner Name", dt[0].P1FirstName + " " + dt[0].P1LastName);
            acroFields.SetField("Owner%", dt[0].P1OwnershipPercent.ToString());
            acroFields.SetField("Owner Birthdate", dt[0].P1DOB);
            //acroFields.SetField("1OWNSOI", dt[0].P1DriversLicenseState);
            acroFields.SetField("Owner License", dt[0].P1DriversLicenseNo);
            acroFields.SetField("Owner License Expiry", dt[0].P1DriversLicenseExp);
            //acroFields.SetField("1OWNHP", dt[0].P1PhoneNumber);

            acroFields.SetField("CertTitle", dt[0].P1Title);
            acroFields.SetField("CertificationTitle", dt[0].P1Title);
            acroFields.SetField("CertificationName", dt[0].P1FullName);

            acroFields.SetField("CertificationTitle2", dt[0].P2Title);
            acroFields.SetField("CertificationName2", dt[0].P2FullName);

            #endregion

            #region Principal #2
            //Principal #2
            acroFields.SetField("Owner ZiP2", dt[0].P2ZipCode);
            acroFields.SetField("Owner State2", dt[0].P2State);
            acroFields.SetField("Owner City2", dt[0].P2City);
            acroFields.SetField("Home Address2", dt[0].p2Address);
            acroFields.SetField("Title2", dt[0].P2Title);
            acroFields.SetField("SSN2", dt[0].P2SSN);
            acroFields.SetField("Owner Name2", dt[0].P2FirstName + " " + dt[0].P2LastName);
            acroFields.SetField("Owner%2", dt[0].P2OwnershipPercent.ToString());
            acroFields.SetField("Owner Birthdate2", dt[0].P2DOB);
            //acroFields.SetField("2OWNSOI", dt[0].P2DriversLicenseState);
            acroFields.SetField("Owner License2", dt[0].P2DriversLicenseNo);
            acroFields.SetField("Owner License Expiry2", dt[0].P2DriversLicenseExp);
            //acroFields.SetField("2OWNHP", dt[0].P2PhoneNumber);
            #endregion

            #region Rates
            //Rates
            acroFields.SetField("Average", dt[0].AverageTicket.ToString());
            acroFields.SetField("MonthlyVolume", dt[0].MonthlyVolume.ToString());
            acroFields.SetField("StatementFee", dt[0].CustServFee.ToString());
            acroFields.SetField("DiscountUS", dt[0].DiscQNP.ToString());
            acroFields.SetField("MinFee", dt[0].MonMin.ToString());
            acroFields.SetField("CHARF", dt[0].RetrievalFee.ToString());
            acroFields.SetField("ChargebackFee", dt[0].ChargebackFee.ToString());
            acroFields.SetField("AppFee", dt[0].AppFee.ToString());

           
            acroFields.SetField("AVSFee", dt[0].AVS.ToString());
            acroFields.SetField("AuthFee", dt[0].TransactionFee.ToString());
            acroFields.SetField("WireFee", dt[0].BatchHeader.ToString());

            acroFields.SetField("GatewayTxnFee", dt[0].GatewayTransFee.ToString());
            acroFields.SetField("GatewayFee", dt[0].GatewayMonFee.ToString());
            acroFields.SetField("ReserveRate", dt[0].RollingReserve.ToString());

            #endregion

            #region Questionnaire
            acroFields.SetField("SalesMail", dt[0].BusinessPctMail.ToString());
            acroFields.SetField("SalesRetail", dt[0].BusinessPctRetail.ToString());
            acroFields.SetField("Trade Show", dt[0].BusinessPctTradeShows.ToString().ToString());
            acroFields.SetField("SalesNet", dt[0].BusinessPctInternet.ToString().ToString());
            acroFields.SetField("SalesPhone", dt[0].BusinessPctPhone.ToString());

            acroFields.SetField("ProdDescription1", dt[0].ProductSold);

            acroFields.SetField("Physical Address", dt[0].Address1 + " " + dt[0].Address2);
            acroFields.SetField("Physical City", dt[0].CITY);
            acroFields.SetField("PhysicalProv", dt[0].STATE + " / " + dt[0].ZipCode);

            if (dt[0].RefundPolicy.ToLower().Contains("other"))
                acroFields.SetField("Refund1", dt[0].OtherRefund);
            else
                acroFields.SetField("Refund1", dt[0].RefundPolicy);

            //acroFields.SetField("Current Processor", dt[0].CurrProcessor);

            int numDaysDelivered = Convert.ToInt16(dt[0].NumDaysDel);
            if (numDaysDelivered >= 1 && numDaysDelivered <= 7)
                acroFields.SetField("Order.ToString()Time", "1");
            else if (numDaysDelivered >= 8 && numDaysDelivered <= 14)
                acroFields.SetField("Order.ToString()Time", "2");
            else if (numDaysDelivered > 14)
                acroFields.SetField("Order.ToString()Time", "3");


            #endregion

            stamper.FormFlattening = true;
            stamper.Close();
            DisplayMessage("Optimal International PDF created in the customer folder - " + FilePath);
            return 0;
        }//end if count not 0
        else
            return 1;
    }//end function CreateInternationalPDF

    #endregion
    
   
    public bool CreateCanadaPDF(string ContactID)
    {
        PDFBL PDF = new PDFBL();
        PartnerDS.ACTOptimalCAPDFDataTable dt = PDF.GetCanadaPDFFromACT(ContactID);

        if (dt.Rows.Count > 0)
        {
            PdfReader reader = new PdfReader(Server.MapPath("/PartnerPortal/PDF/Optimal Canada App.pdf"));

            ACTDataBL fp = new ACTDataBL();
            string FilePath = fp.ReturnCustomerFilePath(ContactID);
            string strPath = "";
            if (FilePath != string.Empty)
            {
                FilePath = FilePath.ToLower();
                FilePath = FilePath.Replace("file://s:\\customers", "");
                FilePath = FilePath.Replace("\\", "/");

                string strHost = "../../Customers";
                string P1FirstName = dt[0].P1FirstName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1FirstName == "")
                    P1FirstName = "CTC";

                string P1LastName = dt[0].P1LastName;
                //if the Principal's Name is empty, initalize to ECE Merchant
                if (P1LastName == "")
                    P1LastName = "Merchant";

                strPath = Server.MapPath(strHost + FilePath + "/" + "Optimal Canada " + P1FirstName + " " + P1LastName + ".pdf");
            }

            FileStream fStream = null;
            fStream = new FileStream(strPath, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fStream);
            stamper.SetEncryption(PdfWriter.STRENGTH128BITS, "succeed", "succeed", PdfWriter.AllowCopy | PdfWriter.AllowPrinting);


            AcroFields acroFields = stamper.AcroFields;
            #region General Information
            acroFields.SetField("LEGNOB", dt[0].COMPANYNAME);
            acroFields.SetField("DBADBA", dt[0].DBA);
            acroFields.SetField("WHADWYLT", dt[0].DBA);
            if (dt[0].COMPANYNAME == dt[0].DBA)
                acroFields.SetField("SAMADOP", "Yes");
            acroFields.SetField("YOUEA", dt[0].Email);
            acroFields.SetField("YOUWA", dt[0].Website);
            acroFields.SetField("businessType", dt[0].LegalStatus);

            string BillingAddress = "";
            if (dt[0].Address.ToString() == dt[0].BillingAddress)
                acroFields.SetField("HEAOMA.BILAD", "Yes");
            else
            {
                BillingAddress = dt[0].BillingAddress + " " + dt[0].BillingAddress2 + " " +
                                     dt[0].BillingCity + " " + dt[0].BillingState + " " + 
                                     dt[0].BillingZip.ToString() +  " " + dt[0].BillingCountry;
                acroFields.SetField("MailingAddressFull", BillingAddress);
            }
            acroFields.SetField("BUSANPOB", dt[0].Address);
            acroFields.SetField("BUSAC", dt[0].CITY);
            acroFields.SetField("BUSASPR", dt[0].STATE);
            acroFields.SetField("BUSAZ", dt[0].ZipCode);

            acroFields.SetField("YEAIB", dt[0].YIB.ToString());
            if ( dt[0].RefundPolicy != "Other")
                acroFields.SetField("RefundPolicy", dt[0].RefundPolicy);
            else
                acroFields.SetField("RefundPolicy", dt[0].OtherRefund);
     

            string BusPhone = dt[0].BusinessPhone;
            if (BusPhone != "")
            {
                acroFields.SetField("areaCode", BusPhone.Substring(0, 3));
                acroFields.SetField("phonePrefix", BusPhone.Substring(4, 3));
                acroFields.SetField("phonePostfix", BusPhone.Substring(8, 4));
            }

            string CustServPhone = dt[0].CustServPhone;
            if (CustServPhone != "")
            {
                acroFields.SetField("areaCodeCS", CustServPhone.Substring(0, 3));
                acroFields.SetField("PhonePrefixCS", CustServPhone.Substring(4, 3));
                acroFields.SetField("PhonePostfixCS", CustServPhone.Substring(8, 4));
            }

            acroFields.SetField("PROSS", dt[0].ProductSold);
            acroFields.SetField("previousProcessor", dt[0].PrevProcessor);
            acroFields.SetField("leavingReason", dt[0].ReasonForLeaving);
            #endregion


            #region Principal #1
            //Principal #1
            acroFields.SetField("1OWNPOFN", dt[0].P1FullName);
            acroFields.SetField("1OWNZI", dt[0].P1ZipCode);
            acroFields.SetField("1OWNST", dt[0].P1State);
            acroFields.SetField("1OWNCI", dt[0].P1City);
            acroFields.SetField("1OWNHA", dt[0].P1Address);
            acroFields.SetField("1OWNTI", dt[0].P1Title);
            acroFields.SetField("1OWNSSN", dt[0].P1SSN);
            if (dt[0].P1Country == "Canada")
                acroFields.SetField("1OWNPCOR.CANAD", "Yes");
            else if (dt[0].P1Country != "")
                acroFields.SetField("1OWNPCOR.CANAD", "No");

            if (dt[0].CTMF == "Yes")
                acroFields.SetField("OWNPOHYE", "Yes");
            else if (dt[0].CTMF == "No")
                acroFields.SetField("OWNPOHYE", "NO");

            if (dt[0].Bankruptcy == "Yes")
                acroFields.SetField("filedBankruptcy", "Yes");
            else if (dt[0].CTMF == "No")
                acroFields.SetField("filedBankruptcy", "No");

            acroFields.SetField("1OWNPRISO", dt[0].P1OwnershipPercent.ToString());
            acroFields.SetField("1OWNDOBMD", dt[0].P1DOB);

            string P1Phone = dt[0].P1PhoneNumber;
            if (P1Phone != "")
            {
                acroFields.SetField("ownerAreaCode", P1Phone.Substring(0, 3));
                acroFields.SetField("ownerPhonePrefix", P1Phone.Substring(4, 3));
                acroFields.SetField("ownerPhonePostfix", P1Phone.Substring(8, 4));
            }


            string P2Phone = dt[0].p2PhoneNumber;
            if (P2Phone != "")
            {
                acroFields.SetField("partnerAreaCode", P2Phone.Substring(0, 3));
                acroFields.SetField("partnerPhonePrefix", P2Phone.Substring(4, 3));
                acroFields.SetField("partnerPhoneSuffix", P2Phone.Substring(8, 4));
            }
            #endregion

            #region Principal #2
            //Principal #2
            acroFields.SetField("2OWNZI", dt[0].P2ZipCode);
            acroFields.SetField("2OWNST", dt[0].P2State);
            acroFields.SetField("2OWNCI", dt[0].P2City);
            acroFields.SetField("2OWNHA", dt[0].p2Address);
            if (dt[0].P2Country == "Canada")
                acroFields.SetField("2OWNPCOR.CANAD", "Yes");
            else if (dt[0].P2Country != "")
                acroFields.SetField("2OWNPCOR.CANAD", "No");
            acroFields.SetField("2OWNTI", dt[0].P2Title);
            acroFields.SetField("2OWNSSN", dt[0].P2SSN);
            acroFields.SetField("2OWNPOFN", dt[0].P2FullName);
            acroFields.SetField("2OWNPRISO", dt[0].P2OwnershipPercent.ToString());
            acroFields.SetField("2OWNDOBMD", dt[0].P2DOB);

            acroFields.SetField("2OWNHP", dt[0].p2PhoneNumber);
            #endregion

            acroFields.SetField("pctRetail", dt[0].BusinessPctRetail.ToString());
            acroFields.SetField("pctInternet.ToString()", dt[0].BusinessPctInternet.ToString().ToString());
            acroFields.SetField("pctMail", dt[0].BusinessPctMailOrder.ToString().ToString());
            #region Rates
            //Rates
            acroFields.SetField("averageTicketUSD", dt[0].AverageTicket.ToString());
            acroFields.SetField("maximumTicketUSD", dt[0].MaxTicket.ToString());
            acroFields.SetField("monthlyVolumeUSD", dt[0].MonthlyVolume.ToString());
            acroFields.SetField("monthlyMdrUSD", dt[0].MonMin.ToString());

            //Canadian Rates, use same info as US (for now)
            acroFields.SetField("averageTicketCD", dt[0].AverageTicket.ToString());
            acroFields.SetField("maximumTicketCD", dt[0].MaxTicket.ToString());
            acroFields.SetField("monthlyVolumeCD", dt[0].MonthlyVolume.ToString());
            acroFields.SetField("monthlyMdrCD", dt[0].MonMin.ToString());
          
            acroFields.SetField("merchantNumberOFI", dt[0].AmexNum);

            acroFields.SetField("transFee", dt[0].TransFee.ToString());
            acroFields.SetField("NBCTransFee", dt[0].NBCTransFee.ToString());
            acroFields.SetField("NSF", "5.00");           
            acroFields.SetField("monthlyMDRUSD", dt[0].MonMin.ToString());
            acroFields.SetField("custServFee", dt[0].CustServFee.ToString());
            //acroFields.SetField("setupFee", dt[0].SetupFee.ToString());
            acroFields.SetField("chargebackFee", dt[0].ChargebackFee.ToString());
            acroFields.SetField("accountNumberCD", dt[0].BankAccountNumber);
            acroFields.SetField("americanExpress", dt[0].AmexApplied);
            if (dt[0].DiscQNP.ToString() != "")
            {
                acroFields.SetField("DiscRate", dt[0].DiscQNP.ToString());
                acroFields.SetField("DiscRateCD", dt[0].DiscQNP.ToString());    
                acroFields.SetField("setupFee", "60");
            }
            else if (dt[0].DiscQP.ToString() != "")
            {
                acroFields.SetField("DiscRate", dt[0].DiscQP.ToString());
                acroFields.SetField("DiscRateCD", dt[0].DiscQP.ToString());
                acroFields.SetField("setupFee", "50");
            }

            #endregion

            stamper.FormFlattening = true;
            stamper.Writer.CloseStream = false;
            stamper.Close();

            DisplayMessage("Optimal Canada PDF created in customer folder - " + FilePath);


            return true;
        }//end if dataset count not 0
        else
        {
            DisplayMessage("Optimal Canada PDF could not be created. ");
            return false;
        }
    }//end function CreateCanadaPDF
    
}
