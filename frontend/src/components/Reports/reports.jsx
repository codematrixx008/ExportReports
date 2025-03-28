import React, { useEffect, useState } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import Paper from '@mui/material/Paper';
// import './Css/reports.css';
import './Style.css';
import axios from 'axios';
import ActionComponent from './ActionComponent';

export default function Reports() {
  const [reportsData, setReportsData] = useState([]);

  useEffect(() => {
    axios
      .get('http://localhost:5280/Reports')
      .then((response) => {
        setReportsData(response.data);
        console.log(response, "reportdattaaa");
      });
  }, []);
  
  const rows = reportsData || [];

  const columns = [
    { field: 'reportName', headerName: 'Report Name', flex: 0.7},
    { field: 'lastGeneratedBy', headerName: 'Last GeneratedBy', flex: 0.7},
    { field: 'lastGeneratedOn', headerName: 'Last GeneratedOn', flex: 0.7},
    { field: 'export', headerName: 'Export', flex: 1.0,
      renderCell: (params) => (
        <>
          <ActionComponent cellData={params.row}/>
          {console.log("Param send to ActionComponent", params)}
        </>
      ),
    },
  ];

  const paginationModel = { page: 0, pageSize: 10 };

  return (<div style={{padding:10}}>
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
