namespace OrthogonalRangeQuery
{
    internal class KDTree
    {
        private Nodo ? Raiz { get; set; }
        private List<Punto> Puntos { get; set; }

        public KDTree() 
        {
            this.Raiz = null;
            this.Puntos = new();
        }

        public void CrearArbol(List<Punto> Pts, int comp)
        {
            int n = Pts.Count;
            Punto [] Points = new Punto[n];
            List<Punto> Ordenados = new();

            for(int i = 0; i < n; ++i)
            {
                Points[i] = Pts[i];
            }

            if(comp == 0)
            {
                Array.Sort(Points, Punto.CompareX);
            }
            else if(comp == 1)
            {
                Array.Sort(Points, Punto.CompareY);
            }

            foreach(Punto p in Points)
            { 
                Ordenados.Add(p);
            }

            this.Raiz = CrearArbol2(Ordenados);
            this.Puntos = Pts;
        }

        public Nodo CrearArbol2(List<Punto> pts)
        {
            int n = pts.Count;

            if(n == 0)
            {
                return null;
            }
            else if(n == 1)
            {
                return new(pts[0], pts);
            }
            else
            {
                int iMed = n / 2;
                Punto pMed = pts[iMed];
                List<Punto> ptsIzq = new();
                List<Punto> ptsDer = new();

                for(int i = 0; i < iMed; ++i)
                {
                    ptsIzq.Add(pts[i]);
                }
                for(int i = iMed+1; i<n; ++i)
                {
                    ptsDer.Add(pts[i]);
                }

                Nodo split = new(pMed, pts);
                split.Left = CrearArbol2(ptsIzq);
                split.Right = CrearArbol2(ptsDer);

                return split;
            }
        }

        private static bool IsLeaf(Nodo raiz)
        {
            return raiz == null || (raiz.Left == null && raiz.Right == null);
        }

        private static Nodo Buscar(Nodo raiz, Range2D R, int comp)
        {
            Nodo u = raiz;

            if(comp == 0)
            {
                Range1D r = R.RangeX;

                while(!IsLeaf(u) && (u.Pt.x > r.max || u.Pt.x < r.min))
                {
                    if(u.Pt.x >= r.max)
                    {
                        u = u.Left;
                    }
                    else
                    {
                        u = u.Right;
                    }
                }
            }
            else if(comp == 1)
            {
                Range1D r = R.RangeY;

                while(!IsLeaf(u) && (u.Pt.y > r.max || u.Pt.y < r.min))
                {
                    if(u.Pt.y >= r.max)
                    {
                        u = u.Left;
                    }
                    else
                    {
                        u = u.Right;
                    }
                }
            }

            return u;
        }

        private List<Punto> Query1D(Range2D R)
        {
            List<Punto> query = new();
            Nodo split = Buscar(this.Raiz, R, 1);
            Range1D r = R.RangeY;

            if(split != null)
            {
                if (R.Contains(split.Pt))
                {
                    query.Add(split.Pt);
                }

                Nodo v = split.Left;

                while(v != null)
                {
                    if(v.Pt.y >= r.min)
                    {
                        if(R.Contains(v.Pt))
                        {
                            query.Add(v.Pt);
                        }

                        if(v.Right != null)
                        {
                            List<Punto> aux = v.Right.Pts;

                            foreach(Punto p in aux)
                            {
                                if(R.Contains(p))
                                {
                                    query.Add(p);
                                }
                            }
                        }

                        v = v.Left;
                    }
                    else
                    {
                        v = v.Right;
                    }
                }

                v = split.Right;

                while(v != null)
                {
                    if(v.Pt.y <= r.max)
                    {
                        if(R.Contains(v.Pt))
                        {
                            query.Add(v.Pt);
                        }

                        if(v.Left != null)
                        {
                            List<Punto> aux = v.Left.Pts;

                            foreach(Punto p in aux)
                            {
                                if(R.Contains(p))
                                {
                                    query.Add(p);
                                }
                            }
                        }

                        v = v.Right;
                    }
                    else
                    {
                        v = v.Left;
                    }
                }
            }

            return query;
        }

        public void Query2D(Range2D R)
        {
            Nodo split = Buscar(this.Raiz, R, 0);
            List<Punto> query = new();

            ApplicationConfiguration.Initialize();
            Application.Run(new QueryGraph(this.Puntos));

            if(split != null)
            {
                if(R.Contains(split.Pt))
                {
                    query.Add(split.Pt);
                }

                Nodo v = split.Left;

                while(v != null)
                {
                    if(v.Pt.x >= R.RangeX.min)
                    {
                        if(R.Contains(v.Pt))
                        {
                            query.Add(v.Pt);
                        }

                        if(v.Right != null)
                        {
                            if(v.Right.Subtree == null)
                            {
                                v.Right.Subtree = new();
                                v.Right.Subtree.CrearArbol(v.Right.Pts, 1);
                                v.Right.Pts = null;
                            }

                            List<Punto> aux = v.Right.Subtree.Query1D(R);
                            foreach(Punto p in aux)
                            {
                                query.Add(p);
                            }
                        }

                        v = v.Left;
                    }
                    else
                    {
                        v = v.Right;
                    }
                }

                v = split.Right;

                while(v != null)
                {
                    if(v.Pt.x <= R.RangeX.max)
                    {
                        if(R.Contains(v.Pt))
                        {
                            query.Add(v.Pt);
                        }

                        if(v.Left != null)
                        {
                            if(v.Left.Subtree == null)
                            {
                                v.Left.Subtree = new();
                                v.Left.Subtree.CrearArbol(v.Left.Pts, 1);
                                v.Left.Pts = null;
                            }

                            List<Punto> aux = v.Left.Subtree.Query1D(R);
                            foreach(Punto p in aux)
                            {
                                query.Add(p);
                            }
                        }

                        v = v.Right;
                    }
                    else
                    {
                        v = v.Left;
                    }
                }
            }

            Application.Run(new QueryGraph(this.Puntos, R));
            Application.Run(new QueryGraph(this.Puntos, R, query));

            Console.WriteLine("\nQUERY:\n");
            foreach(Punto p in query)
            {
                Console.WriteLine($"({p.x},{p.y})");
            }
            Console.WriteLine($"\n\nQUERY SIZE: {query.Count}");
        }
    }
}