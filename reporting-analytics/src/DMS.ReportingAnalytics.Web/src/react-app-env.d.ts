/// <reference types="react-scripts" />
/// <reference types="react" />
/// <reference types="react-dom" />

interface Window {
  ENV?: {
    REACT_APP_API_URL?: string;
    [key: string]: string | undefined;
  };
}

declare module "*.svg" {
  const content: React.FunctionComponent<React.SVGAttributes<SVGElement>>;
  export default content;
}

declare module "*.png" {
  const content: string;
  export default content;
}

declare module "*.jpg" {
  const content: string;
  export default content;
}

declare module "*.jpeg" {
  const content: string;
  export default content;
}

declare module "*.gif" {
  const content: string;
  export default content;
}

declare module "*.module.css" {
  const classes: { readonly [key: string]: string };
  export default classes;
}

declare module "*.module.scss" {
  const classes: { readonly [key: string]: string };
  export default classes;
}

declare module "*.module.sass" {
  const classes: { readonly [key: string]: string };
  export default classes;
}

// Add this for echarts-for-react
declare module "echarts-for-react" {
  import { ReactElement, Component } from "react";
  import { EChartsOption } from "echarts";

  interface ReactEChartsProps {
    option: EChartsOption;
    style?: React.CSSProperties;
    className?: string;
    theme?: string;
    notMerge?: boolean;
    lazyUpdate?: boolean;
    onEvents?: Record<string, Function>;
  }

  export default class ReactECharts extends Component<ReactEChartsProps> {
    getEchartsInstance: () => any;
  }
}

// Add this for missing react/jsx-runtime
declare namespace JSX {
  interface IntrinsicElements {
    [elemName: string]: any;
  }
}
