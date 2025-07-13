/**
 * Custom type definitions for MUI X DataGrid since the type exports might vary between versions
 */

export interface GridColDef {
  field: string;
  headerName: string;
  width?: number;
  minWidth?: number;
  flex?: number;
  type?: string;
  align?: 'left' | 'right' | 'center';
  headerAlign?: 'left' | 'right' | 'center';
  sortable?: boolean;
  filterable?: boolean;
  renderCell?: (params: any) => React.ReactNode;
  renderHeader?: () => React.ReactNode;
  valueFormatter?: (params: any) => string | number;
  valueGetter?: (params: any) => any;
}

export interface GridRenderCellParams<TRow = any, TValue = any> {
  value: TValue;
  row: TRow;
  api: any;
  id: string | number;
  field: string;
  formattedValue?: string | number;
}

export interface GridValueFormatterParams {
  value: any;
  api: any;
  field: string;
  id?: string | number;
}
