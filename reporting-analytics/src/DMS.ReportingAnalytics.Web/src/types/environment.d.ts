// environment.d.ts
declare global {
  interface Window {
    ENV?: {
      REACT_APP_API_URL?: string;
      [key: string]: any;
    }
  }
}

export {};
