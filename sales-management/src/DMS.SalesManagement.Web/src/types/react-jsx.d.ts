declare namespace React {
  interface FC<P = {}> {
    (props: P): JSX.Element | null;
  }
  
  type ReactNode = any;
  
  function useState<T>(initialState: T | (() => T)): [T, (newState: T | ((prevState: T) => T)) => void];
  function useEffect(effect: () => void | (() => void), deps?: readonly any[]): void;
  function useContext<T>(context: React.Context<T>): T;
  function createContext<T>(defaultValue: T): React.Context<T>;
  function lazy<T extends React.ComponentType<any>>(factory: () => Promise<{ default: T }>): T;
  const StrictMode: any;
  
  interface Context<T> {
    Provider: React.Provider<T>;
    Consumer: React.Consumer<T>;
  }
  
  interface Provider<T> {
    value: T;
  }
  
  interface Consumer<T> {
    (value: T): React.ReactNode;
  }
}

declare module 'react' {
  export = React;
}

declare module 'react/jsx-runtime' {
  export const jsx: any;
  export const jsxs: any;
  export const Fragment: React.ComponentType;
}

declare global {
  namespace JSX {
    interface Element {}
    interface IntrinsicElements {
      [elemName: string]: any;
    }
  }
}
