import React, { useState ,useEffect} from "react";
import { Chip } from "@mui/material";
import axios from "axios";
import { BarLoader } from "react-spinners";
import "./Style.css";
import Avatar from "@mui/material/Avatar";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faFileCsv, faFileExcel, faFileArchive, faRefresh } from "@fortawesome/free-solid-svg-icons";

const ActionComponent = ({ cellData }) => {
  const [loader, setLoader] = useState(false);
  const apiBaseUrl = "http://localhost:5280";


  const handleExport = (cellData, type) => {
    setLoader(true);

    const apiUrl = `${apiBaseUrl}/Reports/ExportToExcelandCsv?ReportName=${cellData?.reportName}&SpName=${cellData?.spName}&ExportType=${type}`;
    console.log("Data sent for SP execution:", apiUrl);

    axios
      .get(apiUrl, { responseType: "blob" })
      .then((response) => {
        setLoader(false);

        const now = new Date();
        const formattedDate = now.toLocaleDateString("en-GB").replace(/\//g, "-");
        const formattedTime = now.toTimeString().split(" ")[0].replace(/:/g, "-");

        const fileExtension = type === "xlsx" ? "xlsx" : type === "csv" ? "csv" : "zip";
        const fileName = `${cellData?.reportName}_${formattedDate}_${formattedTime}.${fileExtension}`;

        console.log("Downloaded filename:", fileName);

        const url = window.URL.createObjectURL(new Blob([response.data]));
        const a = document.createElement("a");
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      })
      .catch((error) => {
        setLoader(false);
        console.error("Error exporting file:", error);
      });
  };
console.log("CellData:",cellData)


  const handleRefresh = (cellData, type) => {
    setLoader(true);
    console.log("onHandleRefresh : ", cellData);

    axios
      .get(`${apiBaseUrl}/Reports/SaveToServerForStaticReport`, {
        params: { ReportId: cellData?.reportID, ReportName: cellData?.reportName, SpName: cellData?.spName, ExportType: type },
      })
      .then((response) => {
        setLoader(false);
        console.log(response.data.message);
        alert(response.data.message);
      })
      .catch((error) => {
        setLoader(false);
        console.error("Error saving file:", error.response?.data || error.message);
        alert("Error saving file. Check server logs.");
      });
  };

  const handleDownload = async (cellData, type) => {
    try {
      console.log("cellData:", cellData.reportFileName, type, cellData);
      if (!cellData?.reportFileName) {
        console.error("File name is required.");
        return;
      }

      const fileName = `${cellData.reportFileName}.${type}`;
      const response = await fetch(`${apiBaseUrl}/Reports/DownloadReportFile?fileName=${encodeURIComponent(fileName)}`);

      if (!response.ok) {
        throw new Error(`Failed to download file: ${response.statusText}`);
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error downloading file:", error);
    }
  };

  return (
    <>
      <div className="export-field">
        {/* Excel */}

        <Chip
          sx={{ marginRight: "10px", backgroundColor: "transparent", boxShadow: "none", border: "none" }}
          avatar={
            <Avatar style={{ backgroundColor: "transparent" }}>
              <FontAwesomeIcon icon={faFileExcel} style={{ color: "darkgreen", fontSize: "1.2rem" }} />
            </Avatar>
          }
          onClick={() => handleDownload(cellData, "xlsx")}
          disabled={loader || cellData.isGenerating}
        />
        {/* CSV */}
        <Chip
          sx={{ marginRight: "10px", backgroundColor: "transparent", boxShadow: "none", border: "none" }}
          avatar={
            <Avatar style={{ backgroundColor: "transparent" }}>
              <FontAwesomeIcon icon={faFileCsv} style={{ color: "green", fontSize: "1.2rem" }} />
            </Avatar>
          }
          onClick={() => handleDownload(cellData, "csv")}
          disabled={loader|| cellData.isGenerating}
        />
        {/* Zip */}
        <Chip
          sx={{ marginRight: "10px", backgroundColor: "transparent", boxShadow: "none", border: "none" }}
          avatar={
            <Avatar style={{ backgroundColor: "transparent" }}>
              <FontAwesomeIcon icon={faFileArchive} style={{ color: "darkorange", fontSize: "1.2rem" }} />
            </Avatar>
          }
          onClick={() => handleDownload(cellData, "zip")}
          disabled={loader|| cellData.isGenerating}
        />
        {/* Refresh */}
        {cellData.hasStaticFile && (
          <Chip
            sx={{ marginRight: "10px", backgroundColor: "transparent", boxShadow: "none", border: "none" }}
            avatar={
              <Avatar style={{ backgroundColor: "transparent" }}>
                <FontAwesomeIcon icon={faRefresh} style={{ color: "black", fontSize: "1.2rem" }} />
              </Avatar>
            }
            onClick={() => handleRefresh(cellData, "refresh")}
          />
        )}

        <BarLoader loading={loader|| cellData.isGenerating} width={50} />

      </div>
    </>
  );
};

export default ActionComponent;
