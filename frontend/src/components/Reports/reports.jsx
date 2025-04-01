import React, { useEffect, useState } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import Paper from '@mui/material/Paper';
// import './Css/reports.css';
import './Style.css';
import axios from 'axios';
import ActionComponent from './ActionComponent';

export default function Reports() {
  const [reportsData, setReportsData] = useState([]);


  const handleGetReports = () => {
    axios
    .get('http://localhost:5280/Reports')
    .then((response) => {
      setReportsData(response.data);
      console.log(response, "reportdattaaa");
    });
  }

  useEffect(() => {
    handleGetReports()
  }, []);


  const [isAnyGenerating, setIsAnyGenerating] = useState(false);

  useEffect(() => {
    // Check if any row has isGenerating = true
    const hasGenerating = reportsData.some((row) => row.isGenerating);
    setIsAnyGenerating(hasGenerating);
  }, [reportsData]);

  useEffect(() => {
    let intervalId= null;

    if (isAnyGenerating) {
      intervalId = setInterval(() => {
        handleGetReports();
      }, 5000);
    } else if (intervalId) {
      clearInterval(intervalId);
    }

    return () => {
      if (intervalId) clearInterval(intervalId);
    };
  }, [isAnyGenerating]);
  
  const rows = reportsData || [];

  const columns = [
    { field: 'reportName', headerName: 'Report Name', flex: 0.7},
    { field: 'reportFileName', headerName: 'File Name', flex: 0.7},
    { field: 'lastGeneratedBy', headerName: 'Last GeneratedBy', flex: 0.7},
    { 
      field: 'lastGeneratedOn', 
      headerName: 'Last Generated On', 
      flex: 0.7,
      renderCell: (params) => (
        params.value ? new Date(params.value).toLocaleString() : ''
      ),
    },
    // { 
    //   field: 'isGenerated', 
    //   headerName: 'IsGenerated', 
    //   flex: 0.3,
    //   headerAlign: 'center', 
    //   align: 'center', 
    //   renderCell: (params) => (
    //     params.value ? (
    //       <span style={{ 
    //         color: 'green', 
    //         fontSize: '16px', 
    //         display: 'flex', 
    //         justifyContent: 'center', 
    //         alignItems: 'center', 
    //         width: '100%' 
    //       }}>
    //         ✔️
    //       </span>
    //     ) : null
    //   ),
    // },
    { field: 'export', headerName: 'Export', flex: 1.0,
      renderCell: (params) => (
        <>
        {/* param.isGenerating && handle */}
          <ActionComponent cellData={params.row}/>
          {console.log("Param send to ActionComponent", params)}
        </>
      ),
    },
   
  ];

  const paginationModel = { page: 0, pageSize: 10 };

  return (
  <div style={{padding:10}}>
  <h3>Reports</h3>
      <hr></hr>
    <div className="report-container">
      
      <Paper>
      <div className="customDataGrid" style={{ height: 400, width: '100%' }}>
        <DataGrid
        className='dataGrid'
          rows={rows}
          columnHeaderHeight={35}
          rowHeight={35}
          columns={columns}
          pageSize={10}
          getRowId={(row) => row.reportID}
          initialState={{ pagination: { paginationModel } }}
          pageSizeOptions={[10, 20, 50, 100]}
        />
        </div>
      </Paper>
    </div>
    </div>
  );
}
